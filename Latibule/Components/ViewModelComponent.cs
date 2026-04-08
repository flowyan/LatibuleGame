using Engine.Core.ECS;
using Engine.Rendering;
using Latibule.Entities;
using OpenTK.Windowing.Common;

namespace Latibule.Components;

public class ViewModelComponent : BaseComponent
{
    public ViewModelComponent()
    {
        RenderLayer = RenderLayer.Viewmodel;
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (Player.Inventory.SelectedItemIndex == -1) return;

        // TODO: this shit dont render after selecting a different item :(
        Player.Inventory.SelectedItem()?.ViewModel?.Renderer?.Render();
    }

    public override void Dispose()
    {
        base.Dispose();
        Player.Inventory.SelectedItem()?.ViewModel?.Renderer?.Dispose();
    }
}