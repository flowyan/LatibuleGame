using System.Numerics;
using Assimp;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Helpers;
using Latibule.Utilities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Latibule.Core.Rendering.Renderer;

public sealed class ModelRenderer : IRenderable, IDisposable
{
    private readonly Shader _shader;
    private readonly Scene _scene;
    private readonly Transform _transform;
    private readonly Texture _fallback;

    private readonly IReadOnlyList<Texture>? _texturesByMaterial;
    private readonly Texture? _forceAllTexture;

    private readonly List<GpuMesh> _meshes = [];

    private readonly List<(GpuMesh mesh, Matrix4 model)> _opaque = new();
    private readonly List<(GpuMesh mesh, Matrix4 model, float sortKey)> _transparent = new();

    public ModelRenderer(
        Shader shader,
        Scene model,
        Transform transform,
        IReadOnlyList<Texture>? texturesByMaterial,
        Texture? forceAllTexture = null)
    {
        _shader = shader;
        _scene = model;
        _transform = transform;

        _fallback = Asseteer.GetTexture(TextureAsset.missing);
        _texturesByMaterial = texturesByMaterial;
        _forceAllTexture = forceAllTexture;

        BuildGpuMeshes(_scene);
    }

    public void Render()
    {
        var camera = LatibuleGame.Player.Camera;

        var rootModel =
            Matrix4.CreateScale(_transform.Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_transform.Rotation.Z)) *
            Matrix4.CreateTranslation(_transform.Position);

        _shader.Use();
        _shader.SetUniform("view", camera.View);
        _shader.SetUniform("projection", camera.Projection);
        _shader.SetUniform("viewPos", camera.Position);

        _shader.SetUniform("material.diffuse", 0);
        _shader.SetUniform("material.specular", 0);
        _shader.SetUniform("material.shininess", 0.0f);

        LightRenderer.Render(_shader);

        _opaque.Clear();
        _transparent.Clear();

        // collect
        CollectNode(_scene.RootNode, Matrix4.Identity, rootModel, camera.Position);

        // draw opaque
        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true);

        foreach (var cmd in _opaque)
            Draw(cmd.mesh, cmd.model);

        // draw transparent
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        // sort back-to-front
        _transparent.Sort((a, b) => b.sortKey.CompareTo(a.sortKey));
        foreach (var cmd in _transparent)
            Draw(cmd.mesh, cmd.model);

        GL.DepthMask(true);
    }

    private void CollectNode(Node node, Matrix4 parent, Matrix4 rootModel, Vector3 camPos)
    {
        var local = ToOpenTK(node.Transform);
        var global = parent * local;

        if (node.HasMeshes)
        {
            foreach (var meshIndex in node.MeshIndices)
            {
                var mesh = _meshes[meshIndex];
                var model = rootModel * global;

                if (mesh.Mode == RenderMode.Transparent)
                {
                    // use translation as rough center; good enough for most cases
                    var worldPos = model.ExtractTranslation();
                    float key = (worldPos - camPos).LengthSquared;
                    _transparent.Add((mesh, model, key));
                }
                else
                {
                    _opaque.Add((mesh, model));
                }
            }
        }

        for (int i = 0; i < node.ChildCount; i++)
            CollectNode(node.Children[i], global, rootModel, camPos);
    }

    private void Draw(GpuMesh mesh, Matrix4 model)
    {
        _shader.SetUniform("model", model);

        GL.ActiveTexture(TextureUnit.Texture0);

        Texture tex = _fallback;

        // 1) If forced, always use it
        if (_forceAllTexture != null)
        {
            tex = _forceAllTexture;
        }
        // 2) Else use per-material if available
        else if (_texturesByMaterial != null &&
                 mesh.MaterialIndex >= 0 &&
                 mesh.MaterialIndex < _texturesByMaterial.Count)
        {
            tex = _texturesByMaterial[mesh.MaterialIndex];
        }

        tex.Bind();

        mesh.Vao.Bind();
        GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    private void BuildGpuMeshes(Scene scene)
    {
        _meshes.Clear();
        _meshes.Capacity = scene.MeshCount;

        for (int i = 0; i < scene.MeshCount; i++)
        {
            var m = scene.Meshes[i];

            var mode = RenderMode.Opaque;
            if (_texturesByMaterial != null && m.MaterialIndex >= 0 && m.MaterialIndex < _texturesByMaterial.Count)
            {
                var tex = _texturesByMaterial[m.MaterialIndex];
                if (tex.HasTransparency)
                    mode = RenderMode.Transparent; // start here; upgrade to Cutout later if you want
            }

            bool hasNormals = m.HasNormals;
            bool hasUV0 = m.HasTextureCoords(0);

            var verts = new VertexPositionTextureNormal[m.VertexCount];
            for (int v = 0; v < m.VertexCount; v++)
            {
                var p = m.Vertices[v].ToOpenTK();
                var n = hasNormals ? m.Normals[v].ToOpenTK() : Vector3.UnitY;

                Vector2 uv = Vector2.Zero;
                if (hasUV0)
                {
                    var uvw = m.TextureCoordinateChannels[0][v].ToOpenTK();
                    uv = new Vector2(uvw.X, 1f - uvw.Y); // same flip you had
                }

                verts[v] = new VertexPositionTextureNormal(
                    p.X, p.Y, p.Z,
                    uv.X, uv.Y,
                    n.X, n.Y, n.Z
                );
            }

            // Build triangle index list (works even if faces not triangulated: fan triangulation fallback)
            var indices = new List<uint>(m.FaceCount * 3);
            foreach (var face in m.Faces)
            {
                if (face.IndexCount < 3) continue;

                uint i0 = (uint)face.Indices[0];
                for (int k = 1; k + 1 < face.IndexCount; k++)
                {
                    indices.Add(i0);
                    indices.Add((uint)face.Indices[k]);
                    indices.Add((uint)face.Indices[k + 1]);
                }
            }

            var indexArray = indices.ToArray();

            var vao = new VertexArrayObject(sizeof(float) * 8); // 3 pos + 2 uv + 3 normal
            vao.Bind();

            var vbo = new BufferObject<VertexPositionTextureNormal>(verts.Length, BufferTarget.ArrayBuffer, false);
            vbo.SetData(verts, 0, verts.Length);

            var ebo = new BufferObject<uint>(indexArray.Length, BufferTarget.ElementArrayBuffer, false);
            ebo.SetData(indexArray, 0, indexArray.Length);

            // Same attrib layout as your shapes renderer
            var posLoc = _shader.GetAttribLocation("aPos");
            vao.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0);

            var normalLoc = _shader.GetAttribLocation("aNormal");
            vao.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5);

            var uvLoc = _shader.GetAttribLocation("aTexCoords");
            vao.VertexAttribPointer(uvLoc, 2, VertexAttribPointerType.Float, false, sizeof(float) * 3);

            _meshes.Add(new GpuMesh(vao, vbo, ebo, indexArray.Length, m.MaterialIndex, mode));
        }
    }

    private static Matrix4 ToOpenTK(Matrix4x4 m)
    {
        return new Matrix4(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
        );
    }

    public void Dispose()
    {
        foreach (var m in _meshes)
            m.Dispose();
        _meshes.Clear();

        GC.SuppressFinalize(this);
    }

    private sealed class GpuMesh(
        VertexArrayObject vao,
        BufferObject<VertexPositionTextureNormal> vbo,
        BufferObject<uint> ebo,
        int indexCount,
        int materialIndex,
        RenderMode mode)
        : IDisposable
    {
        public readonly VertexArrayObject Vao = vao;
        public readonly int IndexCount = indexCount;
        public readonly int MaterialIndex = materialIndex;
        public readonly RenderMode Mode = mode;

        public void Dispose()
        {
            vbo.Dispose();
            ebo.Dispose();
            Vao.Dispose();
        }
    }

    private enum RenderMode
    {
        Opaque,
        Transparent,
    }
}