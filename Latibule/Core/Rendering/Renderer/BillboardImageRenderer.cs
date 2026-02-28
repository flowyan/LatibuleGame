using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Helpers;
using Latibule.Core.Rendering.Shapes;
using Latibule.Core.Types;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Renderer;

public class BillboardImageRenderer : IRenderable, IDisposable
{
    private Shader _shader = null!;
    private Texture? _texture;
    private Transform _transform;
    private BillboardEnum _billboard;

    private BufferObject<VertexPositionTextureNormal>? _vertexBuffer;
    private BufferObject<uint>? _indexBuffer;
    private VertexArrayObject? _vao;
    private int _indexCount;

    public BillboardImageRenderer(Transform transform, Texture? texture, BillboardEnum billboard = BillboardEnum.Full)
    {
        var shape = new PlaneWallShape();

        _shader = Asseteer.GetShader(ShaderAsset.mesh_shader);
        _transform = transform;
        _texture = texture ?? Asseteer.GetTexture(TextureAsset.missing);
        _billboard = billboard;
        var vertices = new VertexPositionTextureNormal[shape.Vertices.Length];

        for (var i = 0; i < shape.Vertices.Length; i++)
        {
            vertices[i] = new VertexPositionTextureNormal(
                shape.Vertices[i].X,
                shape.Vertices[i].Y,
                shape.Vertices[i].Z,
                shape.Texcoords[i].X * 1,
                shape.Texcoords[i].Y *1,
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
        var camera = LatibuleGame.Player.Camera;
        var model =
            Matrix4.CreateScale(_transform.Scale) *
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_transform.Rotation.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_transform.Rotation.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_transform.Rotation.Z)) *
            Matrix4.CreateTranslation(_transform.Position); // Must be last, order matters in matrices

        var invView = camera.View.Inverted();
        var right = invView.Row0.Xyz;
        var fwd = -invView.Row2.Xyz;
        var up = invView.Row1.Xyz;

        var fullBillboard = new Matrix4(
            new Vector4(right, 0),
            new Vector4(up, 0),
            new Vector4(fwd, 0),
            Vector4.UnitW
        );

        var yLockedBillboard = new Matrix4(
            Vector4.UnitY,
            new Vector4(fwd, 0),
            new Vector4(right, 0),
            Vector4.UnitW
        );

        var modelToUse = _billboard switch
        {
            BillboardEnum.Full => fullBillboard * model,
            BillboardEnum.YLocked => yLockedBillboard * model,
            _ => model
        };

        _shader.Use();
        _shader.SetUniform("model", modelToUse);
        _shader.SetUniform("view", camera.View);
        _shader.SetUniform("projection", camera.Projection);

        ApplyMaterial();

        _shader.SetUniform("viewPos", camera.Position); // todo: this could not be in all shaders

        LightRenderer.Render(_shader);

        _texture?.Bind();
        _vao?.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
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