using Latibule.Core.ECS;
using Latibule.Core.Physics;
using Latibule.Utilities;
using OpenTK.Mathematics;

namespace Latibule.Core.Components;

/// <summary>
/// Collision component that defines a bounding box for collision detection.
///
/// For now only supports AABB.
/// </summary>
public class CollisionComponent : BaseComponent
{
    public Vector3 Position { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 Rotation { get; set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector3 Center => BoundingBox.Center;

    /// <summary>
    /// Whether this object participates in AABB collision detection with the player.
    /// </summary>
    public bool HasCollision { get; set; } = true;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        Position = Parent?.Transform?.Position ?? Vector3.Zero;
        Scale = Parent?.Transform?.Scale ?? Vector3.One;
        Rotation = Parent?.Transform?.Rotation ?? Vector3.Zero;

        UpdateBoundingBox();
    }

    public void UpdateBoundingBox()
    {
        BoundingBox = AabbHelper.CreateFromCenterRotationScale(
            Position,
            new Vector3(Scale.X, Scale.Y, Scale.Z),
            Rotation
        );
    }
}