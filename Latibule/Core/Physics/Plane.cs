using OpenTK.Mathematics;

namespace Latibule.Core.Physics;

public readonly struct Plane
{
    public Vector3 Normal { get; }
    public float D { get; }

    // Plane equation: dot(Normal, X) + D = 0
    public Plane(Vector3 normal, float d)
    {
        Normal = normal;
        D = d;
    }

    // Create plane from point + normal (common)
    public static Plane FromPointNormal(Vector3 point, Vector3 normal)
    {
        normal = normal.Normalized();
        float d = -Vector3.Dot(normal, point);
        return new Plane(normal, d);
    }
}