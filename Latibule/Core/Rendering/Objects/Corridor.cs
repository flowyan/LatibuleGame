using Latibule.Models;
using Microsoft.Xna.Framework;

namespace Latibule.Core.Rendering.Objects;

public class Corridor(Game game) : GameObject(game)
{
    private readonly Game _game = game;

    public override void Initialize()
    {
        var localScale = new Vector3(2, 0, 2);
        var uvScale = new Vector2(1, 1);

        Children =
        [
            new Plane(_game) { Position = new Vector3(0, 0, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale },
            new Plane(_game) { Position = new Vector3(2, 0, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale },
            new Plane(_game) { Position = new Vector3(-2, 0, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale },
            new Plane(_game) { Position = new Vector3(0, 2, 0) + Position, Rotation = new Vector3(0, 0, 0), Scale = localScale, UVScale = uvScale },
        ];

        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
}