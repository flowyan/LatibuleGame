using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace Latibule.Core.Rendering.Helpers;

public class BufferObject<T> : IDisposable where T : unmanaged
{
    private readonly int _handle;
    private readonly BufferTarget _bufferTarget;
    private readonly int _size;

    public unsafe BufferObject(int size, BufferTarget bufferTarget, bool isDynamic)
    {
        _bufferTarget = bufferTarget;
        _size = size;

        _handle = GL.GenBuffer();
        GLUtility.CheckError();

        Bind();

        var elementSizeInBytes = Marshal.SizeOf<T>();
        GL.BufferData(bufferTarget, size * elementSizeInBytes, IntPtr.Zero, isDynamic ? BufferUsageHint.StreamDraw : BufferUsageHint.StaticDraw);
    }

    public void Bind()
    {
        GL.BindBuffer(_bufferTarget, _handle);
        GLUtility.CheckError();
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_handle);
        GLUtility.CheckError();
        GC.SuppressFinalize(this);
    }

    public unsafe void SetData(T[] data, int startIndex, int elementCount)
    {
        Bind();

        fixed (T* dataPtr = &data[startIndex])
        {
            var elementSizeInBytes = Marshal.SizeOf<T>();
            GL.BufferSubData(_bufferTarget, IntPtr.Zero, elementCount * elementSizeInBytes, new IntPtr(dataPtr));
            GLUtility.CheckError();
        }
    }
}