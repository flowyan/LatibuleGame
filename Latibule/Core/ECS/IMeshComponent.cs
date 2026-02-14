using Latibule.Core.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Latibule.Core.ECS;

public class MeshComponent : BaseComponent
{
    public VerticesUV[] Vertices { get; protected set; }
    public uint[] Indices { get; protected set; }

    public int VertexBufferObject { get; protected set; }
    public int ElementBufferObject { get; protected set; }
    public int VertexArrayObject { get; protected set; }

    public override void Dispose()
    {
        base.Dispose();
        GL.DeleteBuffer(VertexBufferObject);
        GL.DeleteBuffer(ElementBufferObject);
        GL.DeleteVertexArray(VertexArrayObject);
        GC.SuppressFinalize(this);
    }
}