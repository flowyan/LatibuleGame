using Engine;
using Engine.Core.ECS;
using Engine.Utilities;
using OpenTK.Mathematics;
using Ray = Engine.Physics.Ray;

namespace Editor.Core;

public static class MousePicking
{
    public static Ray CreateRayFromMouse(
        Vector2 mousePositionInViewport,
        Vector2 viewportSize,
        Matrix4 view,
        Matrix4 projection)
    {
        // Convert viewport pixel coords to OpenGL NDC coordinates.
        var x = (2f * mousePositionInViewport.X) / viewportSize.X - 1f;
        var y = 1f - (2f * mousePositionInViewport.Y) / viewportSize.Y;

        var rayClipNear = new Vector4(x, y, -1f, 1f);
        var rayClipFar = new Vector4(x, y, 1f, 1f);

        var invViewProj = Matrix4.Invert(view * projection);

        var nearWorld4 = Vector4.TransformRow(rayClipNear, invViewProj);
        var farWorld4 = Vector4.TransformRow(rayClipFar, invViewProj);

        nearWorld4 /= nearWorld4.W;
        farWorld4 /= farWorld4.W;

        var nearWorld = nearWorld4.Xyz;
        var farWorld = farWorld4.Xyz;

        var direction = (farWorld - nearWorld).Normalized();
        return new Ray(nearWorld, direction);
    }

    public static bool TryIntersectPhysicsBody(
        Ray rayInput,
        IEnumerable<GameObject> objects,
        out GameObject? hitObject,
        out Vector3 hitPoint
    )
    {
        hitObject = null;
        hitPoint = Vector3.Zero;

        var physicsSystem = LatibuleEngine.Physics.PhysicsSystem;
        var directionWithLength = Vector3.Normalize(rayInput.Direction) * 1000f;
        var ray = new JoltPhysicsSharp.Ray(rayInput.Position.ToNumerics(), directionWithLength.ToNumerics());

        var hasHit = physicsSystem.NarrowPhaseQuery.CastRay(
            ray,
            out var hitResult,
            null,
            null,
            null
        );

        if (hasHit)
        {
            var hitFraction = hitResult.Fraction;
            var intersectionPoint = ray.GetPointOnRay(hitFraction);

            hitPoint = intersectionPoint.ToOpenTK();
        }

        // this is where we get what object has the bodyid we hit
        // (there should be a key value dictionary in the future with bodyid as key and gameobject as value to speed this up)
        foreach (var obj in objects)
        {
            var bodyId = obj.PhysicsBodyID;
            if (bodyId is null)
                continue;

            if (bodyId.Value != hitResult.BodyID) continue;
            hitObject = obj;
            return true;
        }

        return hitObject is not null;
    }
}