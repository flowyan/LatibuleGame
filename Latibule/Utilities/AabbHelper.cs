using Latibule.Core.Physics;
using OpenTK.Mathematics;

namespace Latibule.Utilities;

public static class AabbHelper
{
    public static Vector3 GetPenetration(BoundingBox box, BoundingBox other)
    {
        if (!box.Intersects(other))
            return Vector3.Zero;

        float dx1 = other.Max.X - box.Min.X;
        float dx2 = box.Max.X - other.Min.X;
        float dy1 = other.Max.Y - box.Min.Y;
        float dy2 = box.Max.Y - other.Min.Y;
        float dz1 = other.Max.Z - box.Min.Z;
        float dz2 = box.Max.Z - other.Min.Z;

        float px = (dx1 < dx2) ? dx1 : -dx2;
        float py = (dy1 < dy2) ? dy1 : -dy2;
        float pz = (dz1 < dz2) ? dz1 : -dz2;

        float absX = System.Math.Abs(px);
        float absY = System.Math.Abs(py);
        float absZ = System.Math.Abs(pz);

        if (absX < absY && absX < absZ)
            return new Vector3(px, 0, 0);
        else if (absY < absZ)
            return new Vector3(0, py, 0);
        else
            return new Vector3(0, 0, pz);
    }

    public static float? IntersectionDistance(BoundingBox box, Ray ray)
    {
        Vector3 invDir = new Vector3(
            1f / ray.Direction.X,
            1f / ray.Direction.Y,
            1f / ray.Direction.Z
        );

        Vector3 t1 = (box.Min - ray.Position) * invDir;
        Vector3 t2 = (box.Max - ray.Position) * invDir;

        Vector3 tMin = Vector3.ComponentMin(t1, t2);
        Vector3 tMax = Vector3.ComponentMax(t1, t2);

        float tNear = System.Math.Max(tMin.X, System.Math.Max(tMin.Y, tMin.Z));
        float tFar = System.Math.Min(tMax.X, System.Math.Min(tMax.Y, tMax.Z));

        if (tNear > tFar || tFar < 0)
            return null;

        return tNear >= 0 ? tNear : tFar;
    }

    public static bool RayIntersectsAabb(Vector3 rayStart, Vector3 rayEnd, BoundingBox box, out Vector3 hitPoint,
        out Vector3 hitNormal)
    {
        hitPoint = Vector3.Zero;
        hitNormal = Vector3.Zero;
        if (box == BoundingBox.Empty) return false;

        Vector3 direction = rayEnd - rayStart;
        float length = direction.Length;
        direction.Normalize();

        Ray ray = new Ray(rayStart, direction);
        float? distance = IntersectionDistance(box, ray);

        if (!(distance <= length)) return false;

        hitPoint = rayStart + direction * distance.Value;

        // Calculate normal based on hit face
        Vector3 localPoint = hitPoint - box.Min;
        Vector3 halfSize = (box.Max - box.Min) / 2f;
        Vector3 center = box.Min + halfSize;
        Vector3 delta = hitPoint - center;

        float bias = 0.001f; // Small bias to prevent floating point errors

        if (System.Math.Abs(delta.X) >= halfSize.X - bias)
            hitNormal = new Vector3(System.Math.Sign(delta.X), 0, 0);
        else if (System.Math.Abs(delta.Y) >= halfSize.Y - bias)
            hitNormal = new Vector3(0, System.Math.Sign(delta.Y), 0);
        else if (System.Math.Abs(delta.Z) >= halfSize.Z - bias)
            hitNormal = new Vector3(0, 0, System.Math.Sign(delta.Z));
        else
            hitNormal = Vector3.Zero;

        return true;
    }

    public static BoundingBox CreateFromCenterRotationScale(
        Vector3 center,
        Vector3 halfExtents,
        Vector3 rotationDegrees)
    {
        // Build rotation matrix (rotation only!)
        var rot =
            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotationDegrees.X)) *
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotationDegrees.Y)) *
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotationDegrees.Z));

        // Absolute rotation matrix (ignore translation row/column)
        var absRot = new Matrix4(
            Math.Abs(rot.M11), Math.Abs(rot.M12), Math.Abs(rot.M13), 0,
            Math.Abs(rot.M21), Math.Abs(rot.M22), Math.Abs(rot.M23), 0,
            Math.Abs(rot.M31), Math.Abs(rot.M32), Math.Abs(rot.M33), 0,
            0, 0, 0, 1
        );

        // Compute new half extents
        Vector3 newHalfExtents = new Vector3(
            absRot.M11 * halfExtents.X + absRot.M12 * halfExtents.Y + absRot.M13 * halfExtents.Z,
            absRot.M21 * halfExtents.X + absRot.M22 * halfExtents.Y + absRot.M23 * halfExtents.Z,
            absRot.M31 * halfExtents.X + absRot.M32 * halfExtents.Y + absRot.M33 * halfExtents.Z
        );

        return new BoundingBox(
            center - newHalfExtents,
            center + newHalfExtents
        );
    }
}