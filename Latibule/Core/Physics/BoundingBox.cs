using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace Latibule.Core.Physics;

public readonly struct BoundingBox : IEquatable<BoundingBox>
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = Vector3.ComponentMin(min, max);
        Max = Vector3.ComponentMax(min, max);
    }

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;
    public Vector3 HalfExtents => (Max - Min) * 0.5f;

    public static readonly BoundingBox Empty = new BoundingBox(Vector3.Zero, Vector3.Zero);

    public bool Intersects(in BoundingBox other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    public ContainmentType Contains(in Vector3 point)
    {
        if (point.X < Min.X || point.X > Max.X ||
            point.Y < Min.Y || point.Y > Max.Y ||
            point.Z < Min.Z || point.Z > Max.Z)
            return ContainmentType.Disjoint;

        return ContainmentType.Contains;
    }

    public ContainmentType Contains(in BoundingBox other)
    {
        if (!Intersects(other))
            return ContainmentType.Disjoint;

        if (other.Min.X >= Min.X && other.Max.X <= Max.X &&
            other.Min.Y >= Min.Y && other.Max.Y <= Max.Y &&
            other.Min.Z >= Min.Z && other.Max.Z <= Max.Z)
            return ContainmentType.Contains;

        return ContainmentType.Intersects;
    }

    public ContainmentType Contains(in BoundingSphere sphere)
    {
        Vector3 closest = new Vector3(
            Math.Clamp(sphere.Center.X, Min.X, Max.X),
            Math.Clamp(sphere.Center.Y, Min.Y, Max.Y),
            Math.Clamp(sphere.Center.Z, Min.Z, Max.Z)
        );

        float distSq = (sphere.Center - closest).LengthSquared;
        float rSq = sphere.Radius * sphere.Radius;

        if (distSq > rSq) return ContainmentType.Disjoint;

        if (sphere.Center.X - sphere.Radius >= Min.X && sphere.Center.X + sphere.Radius <= Max.X &&
            sphere.Center.Y - sphere.Radius >= Min.Y && sphere.Center.Y + sphere.Radius <= Max.Y &&
            sphere.Center.Z - sphere.Radius >= Min.Z && sphere.Center.Z + sphere.Radius <= Max.Z)
            return ContainmentType.Contains;

        return ContainmentType.Intersects;
    }

    public bool Intersects(in BoundingSphere sphere)
    {
        Vector3 closest = new Vector3(
            Math.Clamp(sphere.Center.X, Min.X, Max.X),
            Math.Clamp(sphere.Center.Y, Min.Y, Max.Y),
            Math.Clamp(sphere.Center.Z, Min.Z, Max.Z)
        );

        return (sphere.Center - closest).LengthSquared <= sphere.Radius * sphere.Radius;
    }

    public PlaneIntersectionType Intersects(in Plane plane)
    {
        Vector3 n = plane.Normal;

        Vector3 positive = new Vector3(
            n.X >= 0 ? Max.X : Min.X,
            n.Y >= 0 ? Max.Y : Min.Y,
            n.Z >= 0 ? Max.Z : Min.Z
        );

        Vector3 negative = new Vector3(
            n.X >= 0 ? Min.X : Max.X,
            n.Y >= 0 ? Min.Y : Max.Y,
            n.Z >= 0 ? Min.Z : Max.Z
        );

        float distPos = Vector3.Dot(n, positive) + plane.D;
        if (distPos > 0f) return PlaneIntersectionType.Front;

        float distNeg = Vector3.Dot(n, negative) + plane.D;
        if (distNeg < 0f) return PlaneIntersectionType.Back;

        return PlaneIntersectionType.Intersecting;
    }

    public float? Intersects(in Ray ray)
    {
        float tmin = 0f;
        float tmax = float.PositiveInfinity;

        if (!Slab(ray.Position.X, ray.Direction.X, Min.X, Max.X, ref tmin, ref tmax)) return null;
        if (!Slab(ray.Position.Y, ray.Direction.Y, Min.Y, Max.Y, ref tmin, ref tmax)) return null;
        if (!Slab(ray.Position.Z, ray.Direction.Z, Min.Z, Max.Z, ref tmin, ref tmax)) return null;

        if (tmax < 0f) return null;
        return tmin >= 0f ? tmin : tmax;
    }

    private static bool Slab(float ro, float rd, float mn, float mx, ref float tmin, ref float tmax)
    {
        const float eps = 1e-8f;

        if (MathF.Abs(rd) < eps)
        {
            return ro >= mn && ro <= mx;
        }

        float inv = 1f / rd;
        float t1 = (mn - ro) * inv;
        float t2 = (mx - ro) * inv;

        if (t1 > t2) (t1, t2) = (t2, t1);

        tmin = MathF.Max(tmin, t1);
        tmax = MathF.Min(tmax, t2);

        return tmin <= tmax;
    }

    // XNA corner order
    public Vector3[] GetCorners()
    {
        return new[]
        {
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Min.Z),
        };
    }

    public void GetCorners(Vector3[] corners)
    {
        if (corners is null) throw new ArgumentNullException(nameof(corners));
        if (corners.Length < 8) throw new ArgumentException("Array must contain at least 8 elements.", nameof(corners));

        corners[0] = new Vector3(Min.X, Max.Y, Max.Z);
        corners[1] = new Vector3(Max.X, Max.Y, Max.Z);
        corners[2] = new Vector3(Max.X, Min.Y, Max.Z);
        corners[3] = new Vector3(Min.X, Min.Y, Max.Z);
        corners[4] = new Vector3(Min.X, Max.Y, Min.Z);
        corners[5] = new Vector3(Max.X, Max.Y, Min.Z);
        corners[6] = new Vector3(Max.X, Min.Y, Min.Z);
        corners[7] = new Vector3(Min.X, Min.Y, Min.Z);
    }

    public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
    {
        if (points is null) throw new ArgumentNullException(nameof(points));

        bool any = false;
        Vector3 min = default, max = default;

        foreach (var p in points)
        {
            if (!any)
            {
                min = p;
                max = p;
                any = true;
                continue;
            }

            min = Vector3.ComponentMin(min, p);
            max = Vector3.ComponentMax(max, p);
        }

        if (!any) throw new ArgumentException("Points collection must contain at least one point.", nameof(points));
        return new BoundingBox(min, max);
    }

    public static BoundingBox CreateMerged(in BoundingBox original, in BoundingBox additional)
    {
        return new BoundingBox(
            Vector3.ComponentMin(original.Min, additional.Min),
            Vector3.ComponentMax(original.Max, additional.Max)
        );
    }

    public bool Equals(BoundingBox other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object? obj) => obj is BoundingBox bb && Equals(bb);
    public override int GetHashCode() => HashCode.Combine(Min, Max);
    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);
    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);
}
