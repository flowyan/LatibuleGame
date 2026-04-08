using Engine.Physics;
using OpenTK.Mathematics;

namespace Editor.Core;

public static class MousePicking
{
    public static Ray CreateRayFromMouse(
        Vector2 mousePositionInViewport,
        Vector2 viewportSize,
        Matrix4 view,
        Matrix4 projection)
    {
        // Convert from pixel coords to NDC (-1 to 1).
        // Y is flipped because screen coords usually go top->down,
        // while NDC goes bottom->top.
        float x = (2f * mousePositionInViewport.X) / viewportSize.X - 1f;
        float y = 1f - (2f * mousePositionInViewport.Y) / viewportSize.Y;

        Vector4 rayClipNear = new Vector4(x, y, -1f, 1f);
        Vector4 rayClipFar = new Vector4(x, y, 1f, 1f);

        Matrix4 invViewProj = Matrix4.Invert(view * projection);

        Vector4 nearWorld4 = Vector4.TransformRow(rayClipNear, invViewProj);
        Vector4 farWorld4 = Vector4.TransformRow(rayClipFar, invViewProj);

        nearWorld4 /= nearWorld4.W;
        farWorld4 /= farWorld4.W;

        Vector3 nearWorld = nearWorld4.Xyz;
        Vector3 farWorld = farWorld4.Xyz;

        Vector3 direction = (farWorld - nearWorld).Normalized();

        return new Ray(nearWorld, direction);
    }

    public static bool IntersectPlane(
        Ray ray,
        Vector3 planePoint,
        Vector3 planeNormal,
        out Vector3 hitPoint)
    {
        hitPoint = Vector3.Zero;

        float denom = Vector3.Dot(ray.Direction, planeNormal);
        if (MathF.Abs(denom) < 0.0001f)
            return false;

        float t = Vector3.Dot(planePoint - ray.Position, planeNormal) / denom;
        if (t < 0f)
            return false;

        hitPoint = ray.Position + ray.Direction * t;
        return true;
    }

    public static bool IntersectAabb(
        Ray ray,
        Vector3 min,
        Vector3 max,
        out float distance)
    {
        distance = 0f;

        float tMin = (min.X - ray.Position.X) / ray.Direction.X;
        float tMax = (max.X - ray.Position.X) / ray.Direction.X;
        if (tMin > tMax) (tMin, tMax) = (tMax, tMin);

        float tyMin = (min.Y - ray.Position.Y) / ray.Direction.Y;
        float tyMax = (max.Y - ray.Position.Y) / ray.Direction.Y;
        if (tyMin > tyMax) (tyMin, tyMax) = (tyMax, tyMin);

        if (tMin > tyMax || tyMin > tMax)
            return false;

        if (tyMin > tMin) tMin = tyMin;
        if (tyMax < tMax) tMax = tyMax;

        float tzMin = (min.Z - ray.Position.Z) / ray.Direction.Z;
        float tzMax = (max.Z - ray.Position.Z) / ray.Direction.Z;
        if (tzMin > tzMax) (tzMin, tzMax) = (tzMax, tzMin);

        if (tMin > tzMax || tzMin > tMax)
            return false;

        if (tzMin > tMin) tMin = tzMin;
        if (tzMax < tMax) tMax = tzMax;

        distance = tMin < 0f ? tMax : tMin;
        return distance >= 0f;
    }
}