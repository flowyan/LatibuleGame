using Latibule.Core.Components;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Models;
using Microsoft.Xna.Framework;
using Plane = Latibule.Core.Rendering.Models.Plane;

namespace Latibule.Core.Rendering.Objects;

public class Corridor(Game game) : GameObject(game)
{
    private readonly Game _game = game;

    public override void Initialize()
    {
        var localScale = new Vector3(2, 0, 2);
        var uvScale = new Vector2(1, 1);
        BaseComponent[] components =
        [
            new BasicEffectComponent(_game)
            {
                Texture = AssetManager.GetTexture(TextureAsset.misc_tequila),
                Color = Color.Red.ToVector3()
            }
        ];

        AddChildren([
            new Plane(_game) { Position = new Vector3(0, 0, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new Plane(_game) { Position = new Vector3(0, 2, -2) + Position, Rotation = new Vector3(90, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new Plane(_game) { Position = new Vector3(0, 2, 2) + Position, Rotation = new Vector3(270, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
            new Plane(_game) { Position = new Vector3(0, 4, 0) + Position, Rotation = new Vector3(180, 0, 0), Scale = localScale, UVScale = uvScale, Components = components },
        ]);

        base.Initialize();
    }
}