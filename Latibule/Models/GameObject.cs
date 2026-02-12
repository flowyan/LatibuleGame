using Microsoft.Xna.Framework;

namespace Latibule.Models;

public class GameObject(Game game)
{
    public Vector3 Position { get; set; } = Vector3.Zero;

    public Vector3 Rotation { get; set; } = Vector3.Zero;

    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector2 UVScale { get; set; } = Vector2.One;

    public BoundingBox BoundingBox { get; protected set; }

    public GameObject[] Children { get; set; } = [];

    public virtual void Initialize()
    {
        LatibuleGame.GameWorld.AddObject(this);
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Draw(GameTime gameTime)
    {
        // foreach (var child in Children) child.Draw(gameTime);
    }
}