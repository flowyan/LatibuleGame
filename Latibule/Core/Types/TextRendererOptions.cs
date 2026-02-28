using FontStashSharp;
using OpenTK.Mathematics;

namespace Latibule.Core.Types;

public class TextRendererOptions
{
    public string? text { get; init; }
    public float fontSize { get; set; } = 16;
    public FSColor color = FSColor.White;
    public Vector2 scale { get; set; } = Vector2.One;
    public float characterSpacing { get; set; } = 0f;
    public float lineSpacing { get; set; } = 0f;
    public TextStyle textStyle { get; set; } = TextStyle.None;
    public FontSystemEffect fontSystemEffect { get; init; } = FontSystemEffect.None;
    public int effectAmount { get; init; }
    public BillboardEnum billboard { get; set; }
}