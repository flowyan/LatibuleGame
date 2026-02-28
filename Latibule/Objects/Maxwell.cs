using Latibule.Core;
using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
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
            new ShaderComponent(Asseteer.GetShader(ShaderAsset.mesh_shader)),
            new TextureComponent(Asseteer.GetTextures([TextureAsset.maxwell_maxwell, TextureAsset.maxwell_whiskers])),
            new ModelRendererComponent(Asseteer.GetModel(ModelAsset.maxwell)),
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