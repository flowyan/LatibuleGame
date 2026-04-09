using Engine;
using Engine.Utilities;
using Latibule.Entities;
using OpenTK.Mathematics;

namespace Latibule.Core;

public class PlayerPhysics(Player player)
{
    public void ResolveCollisions()
    {
        const float eps = 0.00001f;


        // if (!player.BoundingBox.Intersects(box)) continue;
        //
        // var penetration = AabbHelper.GetPenetration(player.BoundingBox, box);
        // if (penetration.LengthSquared < eps) continue;
        //
        // var normal = Vector3.Normalize(penetration);
        // player.RawPosition += penetration;
        // player.UpdateBoundingBox();
        //
        // var dotProduct = Vector3.Dot(player.Velocity, normal);
        // if (dotProduct < 0) player.Velocity -= normal * dotProduct;
        // if (normal.Y < -0.7f && player.Velocity.Y > 0) player.Velocity.Y = 0;
    }
}