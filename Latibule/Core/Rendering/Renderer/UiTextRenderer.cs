using FontStashSharp;
using Latibule.Core.Data;
using Latibule.Core.Types;
using Latibule.Utilities;
using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Renderer;

public class UiTextRenderer(TextRendererOptions options)
{
    public void Render(Vector2 position, string? text = null)
    {
        var font = Asseteer.FontSystem.GetFont(options.fontSize);
        Asseteer.FontRenderer.Begin();
        font.DrawText(
            Asseteer.FontRenderer,
            text ?? options.text,
            position.ToNumerics(),
            options.color,
            0f,
            System.Numerics.Vector2.Zero,
            options.scale.ToNumerics(),
            0,
            options.characterSpacing,
            options.lineSpacing,
            options.textStyle,
            options.fontSystemEffect,
            options.effectAmount
        );
        Asseteer.FontRenderer.End();
    }
}