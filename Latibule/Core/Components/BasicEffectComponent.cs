using Latibule.Core.ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Latibule.Core.Components;

public sealed class BasicEffectComponent(Game game) : BaseComponent(game)
{
    public BasicEffect Effect { get; } = new(game.GraphicsDevice)
    {
        TextureEnabled = true,
        LightingEnabled = false,
    };

    public Texture2D? Texture
    {
        get => Effect.Texture;
        set => Effect.Texture = value;
    }

    public Vector3? Color
    {
        get => Effect.EmissiveColor;
        set
        {
            Effect.VertexColorEnabled = true;
            Effect.SpecularColor = (Vector3)value!;
            Effect.DiffuseColor = (Vector3)value!;
            Effect.EmissiveColor = (Vector3)value!;
        }
    }

    public override void Dispose()
    {
        Effect.Dispose();
        base.Dispose();
    }
}