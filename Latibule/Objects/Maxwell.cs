using Engine;
using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using Engine.Data;
using Engine.Data.Shaders;
using Engine.Physics;
using Engine.Utilities;
using JoltPhysicsSharp;
using Latibule.Data.Model;
using Latibule.Data.Texture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Objects;

public class Maxwell(bool spin = false) : GameObject, IPrefab
{
    private float _startY;
    private float _t;

    public override void OnLoad()
    {
        base.OnLoad();

        var model = Asseteer.GetModel(Models.Misc.maxwell);

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(EngineShaders.DefaultShader)),
            new TextureComponent(Asseteer.GetTextures([Textures.Models.Maxwell.maxwell, Textures.Models.Maxwell.whiskers])),
            new ModelRendererComponent(model),
        ]);

        using BodyCreationSettings creationSettings = new(
            new StaticCompoundShape(PhysSettings.StaticCompoundShape(model, [1])),
            Transform.Position.ToNumerics(),
            Transform.Rotation.ToQuaternion(),
            MotionType.Kinematic,
            JoltPhysics.Layers.Moving
        );
        PhysicsBodyID = LatibuleEngine.Physics.BodyInterface.CreateAndAddBody(creationSettings, Activation.DontActivate);

        _startY = Transform.Position.Y;
    }

    public override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        _t += (float)args.Time;

        if (!spin) return;

        const float maxHeight = 0.25f;
        const float speed = 5f;

        var spinage = 1000 * (float)args.Time;
        var pos = Transform.Position;
        pos.Y = _startY + ((MathF.Sin(_t * speed) + 1f) * 0.5f) * maxHeight;
        Transform.Position = pos;
        Transform.Rotation += new Vector3(0f, spinage, 0f);

        if (PhysicsBodyID != null)
            LatibuleEngine.Physics.BodyInterface.SetPositionAndRotation(
                PhysicsBodyID.Value,
                Transform.Position.ToNumerics(),
                Transform.Rotation.ToQuaternion(),
                Activation.DontActivate
            );
    }
}