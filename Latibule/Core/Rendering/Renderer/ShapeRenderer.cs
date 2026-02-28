using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Helpers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Renderer;

public class ShapeRenderer : IRenderable, IDisposable
{
    private Shader _shader = null!;
    private Texture? _texture;
    private Transform _transform;

    private BufferObject<VertexPositionTextureNormal>? _vertexBuffer;
    private BufferObject<uint>? _indexBuffer;
    private VertexArrayObject? _vao;
    private int _indexCount;

    public ShapeRenderer(Shader shader, Shape shape, Transform transform, Texture? texture, Vector2 uvScale, float uvRotation)
    {
        _shader = shader;
        _transform = transform;
        _texture = texture ?? Asseteer.GetTexture(TextureAsset.missing);
        var vertices = new VertexPositionTextureNormal[shape.Vertices.Length];

        // Prepare rotation if needed
        float cos = 0f, sin = 0f;
        var applyRotation = uvRotation != 0f;
        if (applyRotation)
        {
            var radians = MathHelper.DegreesToRadians(uvRotation);
            cos = MathF.Cos(radians);
            sin = MathF.Sin(radians);
        }

        var uvCenter = uvScale * 0.5f;
        for (var i = 0; i < shape.Vertices.Length; i++)
        {
            // Apply UV scale
            float u = shape.Texcoords[i].X * uvScale.X;
            float v = shape.Texcoords[i].Y * uvScale.Y;

            // Apply UV rotation if specified
            if (applyRotation)
            {
                // Translate to origin (center of UV space)
                float translatedU = u - uvCenter.X;
                float translatedV = v - uvCenter.Y;

                // Apply rotation
                float rotatedU = translatedU * cos - translatedV * sin;
                float rotatedV = translatedU * sin + translatedV * cos;

                // Translate back
                u = rotatedU + uvCenter.X;
                v = rotatedV + uvCenter.Y;
            }

            vertices[i] = new VertexPositionTextureNormal(
                shape.Vertices[i].X,
                shape.Vertices[i].Y,
                shape.Vertices[i].Z,
                u,
                v,
                shape.Normals[i].X,
                shape.Normals[i].Y,
                shape.Normals[i].Z
            );
        }

        _indexCount = shape.Indices.Length;

        // Create and setup VAO
        _vao = new VertexArrayObject(sizeof(float) * 8); // 8 floats per vertex (3 pos + 2 uv + 3 normal)
        _vao.Bind();

        // Create and fill vertex buffer
        _vertexBuffer = new BufferObject<VertexPositionTextureNormal>(vertices.Length, BufferTarget.ArrayBuffer, false);
        _vertexBuffer.SetData(vertices, 0, vertices.Length);

        // Create and fill index buffer
        _indexBuffer = new BufferObject<uint>(shape.Indices.Length, BufferTarget.ElementArrayBuffer, false);
        _indexBuffer.SetData(shape.Indices, 0, shape.Indices.Length);

        var positionLocation = _shader.GetAttribLocation("aPos");
        _vao.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 0);

        var normalLocation = _shader.GetAttribLocation("aNormal");
        _vao.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5);

        var texCoordLocation = _shader.GetAttribLocation("aTexCoords");
        _vao.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, sizeof(float) * 3);
    }
    
    public void Render()
    {
        GL.Enable(EnableCap.CullFace);
        var camera = LatibuleGame.Player.Camera;
        var model =
            Matrix4.CreateScale(_transform.Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_transform.Rotation.Z)) *
            Matrix4.CreateTranslation(_transform.Position); // Must be last, order matters in matrices

        _shader.Use();
        _shader.SetUniform("model", model);
        _shader.SetUniform("view", camera.View);
        _shader.SetUniform("projection", camera.Projection);

        ApplyMaterial();

        _shader.SetUniform("viewPos", camera.Position); // todo: this could not be in all shaders

        LightRenderer.Render(_shader);

        _texture?.Bind();
        _vao?.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        GL.Disable(EnableCap.CullFace);
    }

    // todo: implement different materials
    private void ApplyMaterial()
    {
        // Material properties
        _shader.SetUniform("material.diffuse", 0); // Texture unit 0
        _shader.SetUniform("material.specular", 0); // Use same texture for specular
        _shader.SetUniform("material.shininess", 0.0f);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vao?.Dispose();
        // _shader?.Dispose();
    }
}