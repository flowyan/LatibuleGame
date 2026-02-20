using OpenTK.Mathematics;

namespace Latibule.Core.Physics;

public readonly struct BoundingSphere : IBoundingShape, IEquatable<BoundingSphere>
{
    public Vector3 Center { get; }
    public float Radius { get; }

    public BoundingSphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool Equals(BoundingSphere other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);
    public override bool Equals(object? obj) => obj is BoundingSphere other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Center, Radius);
    public static bool operator ==(BoundingSphere left, BoundingSphere right) => left.Equals(right);
    public static bool operator !=(BoundingSphere left, BoundingSphere right) => !left.Equals(right);
}