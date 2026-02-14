using Latibule.Entities;
using Latibule.Utilities;
using OpenTK.Mathematics;

namespace Latibule.Core.Physics;

public class PlayerPhysics(Player player)
{
    public void ResolveCollisions()
    {
        var boxes = LatibuleGame.GameWorld.GetBoundingBoxes();

        const float eps = 0.00001f;

        foreach (var box in boxes)
        {
            if (!player.BoundingBox.Intersects(box)) continue;

            var penetration = AabbHelper.GetPenetration(player.BoundingBox, box);
            if (penetration.LengthSquared < eps) continue;

            var normal = Vector3.Normalize(penetration);
            player.RawPosition += penetration;
            player.UpdateBoundingBox();

            var dotProduct = Vector3.Dot(player.Velocity, normal);
            if (dotProduct < 0) player.Velocity -= normal * dotProduct;
            if (normal.Y < -0.7f && player.Velocity.Y > 0) player.Velocity.Y = 0;
        }
    }
}