using OpenTK.Mathematics;

namespace Latibule.Core.Rendering;

public class VerticesUV
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float U { get; set; }
    public float V { get; set; }

    public VerticesUV(float x, float y, float z, float u, float v)
    {
        X = x;
        Y = y;
        Z = z;
        U = u;
        V = v;
    }

    public VerticesUV(float x, float y, float z) : this(x, y, z, 0, 0)
    {
        X = x;
        Y = y;
        Z = z;
        U = 0;
        V = 0;
    }

    public VerticesUV(float x, float y, float z, Vector2 uv) : this(x, y, z, uv.X, uv.Y)
    {
        X = x;
        Y = y;
        Z = z;
        U = uv.X;
        V = uv.Y;
    }

    public void SetUV(float u, float v)
    {
        U = u;
        V = v;
    }

    public float[] ToArray()
    {
        return [X, Y, Z, U, V];
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }

    public Vector2 ToUV()
    {
        return new Vector2(U, V);
    }
}