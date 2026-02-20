using System.Runtime.InteropServices;

namespace Latibule.Core.Rendering.Helpers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionTexture
{
    public float X;
    public float Y;
    public float Z;
    public float U;
    public float V;

    public VertexPositionTexture(float x, float y, float z, float u, float v)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
    }
}

