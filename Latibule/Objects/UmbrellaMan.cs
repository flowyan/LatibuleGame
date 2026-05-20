using Engine;
using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Shaders;
using Engine.Physics;
using Engine.Utilities;
using JoltPhysicsSharp;
using Latibule.Data;
using Latibule.Data.Model;
using Latibule.Data.Texture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Objects;

public class UmbrellaMan : GameObject, IPrefab
{
    public override void OnLoad()
    {
        base.OnLoad();

        var model = Asseteer.GetModel(Models.Misc.umbrellaman);

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(EngineShaders.DefaultShader)),
            new TextureComponent(Asseteer.GetTextures([Textures.Models.UmbrellaMan.eyes, Textures.Models.UmbrellaMan.body, Textures.Models.UmbrellaMan.eyes])),
            new ModelRendererComponent(model),
        ]);

        // Transform.Scale = new Vector3(1f);
        using BodyCreationSettings creationSettings = new(
            new StaticCompoundShape(PhysSettings.StaticCompoundShape(model)),
            Transform.Position.ToNumerics(),
            Transform.Rotation.ToQuaternion(),
            MotionType.Dynamic,
            JoltPhysics.Layers.Moving
        );
        PhysicsBodyID = LatibuleEngine.Physics.BodyInterface.CreateAndAddBody(creationSettings, Activation.DontActivate);
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (PhysicsBodyID == null) return;

        var pos = LatibuleEngine.Physics.BodyInterface.GetPosition(PhysicsBodyID.Value);
        var rot = LatibuleEngine.Physics.BodyInterface.GetRotation(PhysicsBodyID.Value).ToOpenTKEulerDegrees();

        Transform.Position = pos.ToOpenTK();
        Transform.Rotation = rot;
    }
}