using OpenTK.Graphics.OpenGL4;

namespace Latibule.Core.Rendering.Helpers;

public class VertexArrayObject : IDisposable
{
    private readonly uint _handle;
    private readonly int _stride;

    public VertexArrayObject(int stride)
    {
        if (stride <= 0) throw new ArgumentException("Stride must be greater than zero.", nameof(stride));
        _stride = stride;

        GL.GenVertexArrays(1, out _handle);
        GLUtility.CheckError();
    }

    public void Bind()
    {
        GL.BindVertexArray(_handle);
        GLUtility.CheckError();
    }

    public void VertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int offset)
    {
        GL.VertexAttribPointer(index, size, type, normalized, _stride, (IntPtr)offset);
        GLUtility.CheckError();
        GL.EnableVertexAttribArray(index);
        GLUtility.CheckError();
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(_handle);
        GLUtility.CheckError();
        GC.SuppressFinalize(this);
    }
}