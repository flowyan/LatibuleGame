using Latibule.Core.ECS;
using OpenTK.Windowing.Desktop;

namespace Latibule.Core.Components;

public sealed class ShaderComponent(GameWindow gameWindow) : BaseComponent(gameWindow)
{
    // public BasicEffect Effect { get; } = new(gameWindow.GraphicsDevice)
    // {
    //     TextureEnabled = true,
    //     LightingEnabled = false,
    // };

    // public Texture2D? Texture
    // {
    //     get => Effect.Texture;
    //     set => Effect.Texture = value;
    // }

    // public override void Dispose()
    // {
    //     Effect.Dispose();
    //     base.Dispose();
    // }
}