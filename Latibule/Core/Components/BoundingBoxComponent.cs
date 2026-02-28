using System.Drawing;
using Latibule.Core.ECS;
using Latibule.Core.Physics;
using Latibule.Core.Rendering.Renderer;
using Latibule.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components;

/// <summary>
/// Collision component that defines a bounding box for collision detection.
///
/// For now only supports AABB.
/// </summary>
public class BoundingBoxComponent(Vector3? position = null, Vector3? scale = null, Vector3? rotation = null) : BaseComponent
{
    public Vector3 Position { get; internal set; }
    public Vector3 Scale { get; private set; }
    public Vector3 Rotation { get; internal set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector3 Center => BoundingBox.Center;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        Position = Parent?.Transform?.Position ?? Vector3.Zero;
        Scale = Parent?.Transform?.Scale ?? Vector3.One;
        Rotation = Parent?.Transform?.Rotation ?? Vector3.Zero;

        if (position != null) Position = position.Value;
        if (scale != null) Scale = scale.Value;
        if (rotation != null) Rotation = rotation.Value;

        UpdateBoundingBox();
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
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