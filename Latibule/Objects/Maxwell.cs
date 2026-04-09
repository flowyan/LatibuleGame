using Engine.Components;
using Engine.Core.ECS;
using Engine.Data;
using Engine.Data.Shaders;
using Latibule.Data;
using Latibule.Data.Model;
using Latibule.Data.Texture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Objects;

public class Maxwell(bool spin = false) : GameObject
{
    private float _startY;
    private float _t;
    private BoundingBoxComponent _collision;

    public override void OnLoad()
    {
        base.OnLoad();

        _collision = new BoundingBoxComponent(scale: new Vector3(0.7f, 0.5f, 0.5f));

        WithComponents([
            new ShaderComponent(Asseteer.GetShader(EngineShaders.DefaultShader)),
            new TextureComponent(Asseteer.GetTextures([Textures.Models.Maxwell.maxwell, Textures.Models.Maxwell.whiskers])),
            new ModelRendererComponent(Asseteer.GetModel(Models.Misc.maxwell)),
            _collision
        ]);

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
        _collision.Position = pos;

        Transform.Rotation += new Vector3(0f, spinage, 0f);
        _collision.Rotation = Transform.Rotation;
    }
}