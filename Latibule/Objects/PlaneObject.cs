using Engine;
using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Shaders;
using Engine.Physics;
using Engine.Utilities;
using JoltPhysicsSharp;
using OpenTK.Mathematics;
using PlaneShape = Engine.Rendering.Shapes.PlaneShape;

namespace Latibule.Objects;

/// <summary>
/// A flat plane for ground/floor rendering and collision.
/// </summary>
public class PlaneObject : GameObject, IPrefab
{
    public PlaneObject()
    {
        // Default plane transform
        Transform = new Transform(Vector3.Zero, new Vector3(1, 0, 1), Vector3.Zero);
    }

    public override void OnLoad()
    {
        base.OnLoad();
        
        var scale = new Vector3(Transform.Scale.X, 0f, Transform.Scale.Z);
        BoxShape shape = new(scale.ToNumerics());
        using BodyCreationSettings creationSettings = new(
            shape,
            Transform.Position.ToNumerics(),
            Transform.Rotation.ToQuaternion(),
            MotionType.Static,
            JoltPhysics.Layers.NonMoving
        );
        PhysicsBodyID = LatibuleEngine.Physics.BodyInterface.CreateAndAddBody(creationSettings, Activation.DontActivate);

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(EngineShaders.DefaultShader)),
            new ShapeRendererComponent(new PlaneShape()),
        ]);
    }
}