using System.Drawing;
using Latibule.Core.Data;
using Latibule.Core.ECS;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Renderer;
using Latibule.Core.Types;
using Latibule.Utilities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components.Dev;

public class DebugPointLightRendererOverlay : BaseComponent
{
    private Texture _pointLightTexture;
    private BoundingBoxOutlineRenderer _outlineRenderer;

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);
        RenderLayer = RenderLayer.Transparent;
        _pointLightTexture = Asseteer.GetTexture(TextureAsset.debug_pointlight);
        _outlineRenderer = new BoundingBoxOutlineRenderer();
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (!GameStates.EnabledDebugOverlays[DebugOverlayType.PointLights]) return;

        foreach (var light in LatibuleGame.GameWorld.Lights)
        {
            if (light is null) continue;
            var box = AabbHelper.CreateFromCenterRotationScale(light.Position, new Vector3(0.25f), Vector3.Zero);
            _outlineRenderer.Render(box, Color.DeepPink);

            var _icon = new BillboardImageRenderer(new Transform(light.Position, new Vector3(0.25f)), _pointLightTexture);
            _icon.Render();

            // var _pointLightText = new WorldTextRenderer(new Transform(light.Position), new WorldTextRendererOptions()
            // {
            //     text = "LIGHT",
            //     fontSize = 24,
            //     color = FSColor.DeepPink,
            //     billboard = BillboardEnum.Full,
            //     fontSystemEffect = FontSystemEffect.Stroked,
            //     effectAmount = 2
            // });
            // _pointLightText.Render();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}