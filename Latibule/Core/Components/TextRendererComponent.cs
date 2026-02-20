using System.Numerics;
using FontStashSharp;
using Latibule.Core.ECS;
using Latibule.Core.Rendering.Text;
using Latibule.Utilities;
using OpenTK.Windowing.Common;

namespace Latibule.Core.Components;

public class TextRendererComponent : BaseComponent
{
    private FontStashRenderer _renderer;
    private FontSystem _fontSystem;
    private FSColor? _color;
    private string? _text;
    private Func<string>? _textFunc;
    private float _x;
    private float _y;
    private float _scale;

    // Constructor for static text
    public TextRendererComponent(string text, float x, float y, float scale, FSColor? color = null)
    {
        _text = text;
        _x = x;
        _y = y;
        _scale = scale;
        _color = color;
    }

    // Constructor for dynamic text
    public TextRendererComponent(Func<string> textFunc, float x, float y, float scale, FSColor? color = null)
    {
        _textFunc = textFunc;
        _x = x;
        _y = y;
        _scale = scale;
        _color = color;
    }

    public override void OnLoad(GameObject gameObject)
    {
        base.OnLoad(gameObject);

        _renderer = new FontStashRenderer();

        var settings = new FontSystemSettings
        {
            FontResolutionFactor = 4,
            KernelWidth = 2,
            KernelHeight = 2,
        };

        _fontSystem = new FontSystem(settings);
        _fontSystem.AddFont(File.ReadAllBytes(@"Assets/font/Jersey10.ttf"));

        // Register to render on top of everything
        LatibuleGame.UiTextRenderers.Add(RenderText);
    }

    public override void OnRenderFrame(FrameEventArgs args)
    {
        // Skip normal rendering - we render in the UI pass instead
    }

    private void RenderText(FrameEventArgs args)
    {
        _color ??= FSColor.White;

        var font = _fontSystem.GetFont(32);
        var origin = new Vector2(0, 0);

        // Get text from function if provided, otherwise use static text
        var displayText = _textFunc != null ? _textFunc() : _text;

        _renderer.Begin();
        font.DrawText(_renderer, displayText, new Vector2(_x, _y), _color.Value, 0, origin, new Vector2(_scale, _scale));
        _renderer.End();
    }

    public override void Dispose()
    {
        base.Dispose();
        LatibuleGame.UiTextRenderers.Remove(RenderText);
    }
}