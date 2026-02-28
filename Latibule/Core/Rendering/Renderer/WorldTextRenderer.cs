using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Types;
using Latibule.Utilities;

namespace Latibule.Core.Rendering.Renderer;

public class WorldTextRenderer(TextRendererOptions options)
{
    public void Render(Transform _transform, string? text = null)
    {
        var font = Asseteer.FontSystem.GetFont(options.fontSize);
        var size = font.MeasureString(
            text ?? options.text,
            options.scale.ToNumerics(),
            options.characterSpacing,
            options.lineSpacing,
            options.fontSystemEffect,
            options.effectAmount
        );

        var pos = -size * 0.5f;

        Asseteer.FontRenderer.BeginWorld(_transform, options.billboard);

        font.DrawText(
            Asseteer.FontRenderer,
            options.text,
            pos,
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