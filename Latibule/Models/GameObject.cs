using Microsoft.Xna.Framework;

namespace Latibule.Models;

public class GameObject
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;
    public Vector2 UVScale { get; set; } = Vector2.One;

    public GameObject(Game game)
    {
    }

    public virtual void Initialize()
    {
    }

    public virtual void Draw(GameTime gameTime)
    {
    }
}