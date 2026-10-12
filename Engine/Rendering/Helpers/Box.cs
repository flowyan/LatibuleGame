using OpenTK.Mathematics;

namespace Engine.Physics;

public readonly struct Box
{
    public Vector3 Min { get; }
    public Vector3 Max { get; }

    public Box(Vector3 min, Vector3 max)
    {
        Min = Vector3.ComponentMin(min, max);
        Max = Vector3.ComponentMax(min, max);
    }

    public Vector3 Center => (Min + Max) * 0.5f;
    public Vector3 Size => Max - Min;
    public Vector3 HalfExtents => (Max - Min) * 0.5f;

    public static Box CreateFromCenterAndSize(in Vector3 center, in Vector3 halfExtents)
    {
        var newHalfExtents = new Vector3(halfExtents.X, halfExtents.Y, halfExtents.Z);

        return new Box(
            center - newHalfExtents,
            center + newHalfExtents
        );
    }
    
    public static Box CreateFromPoints(IEnumerable<Vector3> points)
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
        return new Box(min, max);
    }

    public static Box CreateMerged(in Box original, in Box additional)
    {
        return new Box(
            Vector3.ComponentMin(original.Min, additional.Min),
            Vector3.ComponentMax(original.Max, additional.Max)
        );
    }
}
