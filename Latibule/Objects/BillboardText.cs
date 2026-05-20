using Engine.Components;
using Engine.Core.ECS;
using Engine.Core.Types;
using FontStashSharp;

namespace Latibule.Objects;

/// <summary>
/// A billboard text object that always faces the camera, used for rendering text in the world.
/// </summary>
public class BillboardText(string text, BillboardEnum billboard = BillboardEnum.YLocked, FSColor? color = null, float fontSize = 32) : GameObject, IPrefab
{
    public override void OnLoad()
    {
        base.OnLoad();

        WithComponents(new WorldTextRendererComponent(new TextRendererOptions
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