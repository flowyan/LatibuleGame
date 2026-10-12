using Engine.Core.ECS;
using Engine.Data;
using Engine.Data.Cubemaps;
using Engine.Data.Shaders;
using Engine.Rendering.Helpers;
using Engine.Rendering.Shapes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Rendering.Renderer;

public class SkyboxRenderer : IRenderable, IDisposable
{
    private Shader _shader = null!;
    private CubemapTexture? _cubemap;

    private BufferObject<VertexPositionTexture>? _vertexBuffer;
    private BufferObject<uint>? _indexBuffer;
    private VertexArrayObject? _vao;
    private int _indexCount;

    public SkyboxRenderer(CubemapTexture? cubemap)
    {
        _shader = Asseteer.GetShader(EngineShaders.Skybox);
        var shape = new Cube();
        _cubemap = cubemap ?? Asseteer.GetCubemapTexture(EngineCubemaps.skybox_logl);
        var vertices = new VertexPositionTexture[shape.Vertices.Length];

        for (var i = 0; i < shape.Vertices.Length; i++)
        {
            vertices[i] = new VertexPositionTexture(
                shape.Vertices[i].X,
                shape.Vertices[i].Y,
                shape.Vertices[i].Z,
                1f,
                1f
            );
        }

        _indexCount = shape.Indices.Length;

        // Create and setup VAO
        _vao = new VertexArrayObject(sizeof(float) * 5); // 3 for position, 2 for texcoord
        _vao.Bind();

        // Create and fill vertex buffer
        _vertexBuffer = new BufferObject<VertexPositionTexture>(vertices.Length, BufferTarget.ArrayBuffer, false);
        _vertexBuffer.SetData(vertices, 0, vertices.Length);

        // Create and fill index buffer
        _indexBuffer = new BufferObject<uint>(shape.Indices.Length, BufferTarget.ElementArrayBuffer, false);
        _indexBuffer.SetData(shape.Indices, 0, shape.Indices.Length);

        var positionLocation = _shader.GetAttribLocation("aPos");
        _vao.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 0);
    }

    public void Render()
    {
        GL.DepthMask(false); // Disable depth writing
        GL.Disable(EnableCap.CullFace); // Disable culling for skybox
        GL.DepthFunc(DepthFunction.Lequal); // Change depth function to allow skybox to be rendered behind all other geometry
        var camera = LatibuleEngine.Camera;

        _shader.Use();
        _shader.SetUniform("view", new Matrix4(new Matrix3(camera.View))); // Remove translation from view matrix
        _shader.SetUniform("projection", camera.Projection);

        _cubemap?.Bind();
        _vao?.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, 0);
        GL.DepthMask(true);
        GL.Disable(EnableCap.CullFace);
        GL.DepthFunc(DepthFunction.Less);
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _vao?.Dispose();
        // _shader?.Dispose();
    }
}