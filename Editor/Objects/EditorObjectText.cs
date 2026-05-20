using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using FontStashSharp;

namespace Editor.Objects;

public class EditorObjectText(string text, BillboardEnum billboard = BillboardEnum.YLocked, FSColor? color = null, float fontSize = 32) : GameObject, IPrefab
{
    public override void OnLoad()
    {
        base.OnLoad();

        WithComponents(new EditorWorldTextRendererComponent(new TextRendererOptions
        {
            text = text,
            fontSize = fontSize,
            color = color ?? FSColor.White,
            fontSystemEffect = FontSystemEffect.Stroked,
            effectAmount = 4,
            billboard = billboard
        }));
    }
}