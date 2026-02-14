using OpenTK.Graphics.OpenGL;

namespace Latibule.Core.Rendering.Mesh;

public interface IMesh : IDisposable
{
    public VerticesUV[] Vertices { get; protected set; }
    public uint[] Indices { get; protected set; }

    public int VertexBufferObject { get; protected set; }
    public int ElementBufferObject { get; protected set; }
    public int VertexArrayObject { get; protected set; }

    void IDisposable.Dispose()
    {
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GL.DeleteVertexArray(VertexArrayObject);
        GC.SuppressFinalize(this);
    }
}