using OpenTK.Mathematics;

namespace Latibule.Core.Physics;

public readonly struct BoundingSphere
{
    public Vector3 Center { get; }
    public float Radius { get; }

    public BoundingSphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}