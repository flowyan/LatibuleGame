using Latibule.Entities;
using Latibule.Utilities;
using Microsoft.Xna.Framework;

namespace Latibule.Core.Gameplay;

public class Physics(Player player)
{
    public bool Moving => Velocity.Length() > 0;
    public Vector3 Velocity;

    public void ResolveCollisions()
    {
        var boxes = LatibuleGame.GameWorld.GetBoundingBoxes();

        foreach (var box in boxes)
        {
            if (!player.Box.Intersects(box)) continue;

            var penetration = AabbHelper.GetPenetration(player.Box, box);
            if (penetration.LengthSquared() < 0.00001f) continue;

            var normal = Vector3.Normalize(penetration);
            player.RawPosition += penetration;
            player.UpdateBoundingBox();

            var dotProduct = Vector3.Dot(player.Velocity, normal);
            if (dotProduct < 0)
            {
                player.Velocity -= normal * dotProduct;
            }

            if (normal.Y < -0.7f && player.Velocity.Y > 0)
            {
                player.Velocity.Y = 0;
            }
        }
    }
}