using Microsoft.Xna.Framework;

namespace Latibule.Core.ECS;

public abstract class BaseComponent(Game game)
{
    public Game Game { get; } = game;

    public virtual void Initialize()
    {
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Dispose()
    {
    }

    public virtual void Draw(GameTime gameTime)
    {
    }
}