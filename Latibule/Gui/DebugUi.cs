using System.Text;
using FontStashSharp;
using Latibule.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Latibule.Gui;

public class DebugUi
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;

    // Crosshair properties
    private readonly Color _crosshairColor = Color.White;

    private const int CrosshairSize = 16;
    private const int CrosshairThickness = 4;
    public bool ShowDebug = Environment.GetEnvironmentVariable("debug") == "true";

    public DebugUi(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);

        // Create a 1x1 white texture for drawing lines
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public void Draw(GameTime gameTime)
    {
        if (!GameStates.ShowHud) return;

        // Begin the sprite batch
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, new RasterizerState());

        DrawCrosshair();
        DrawText($"{Metadata.GAME_NAME} {Metadata.GAME_VERSION}", new Vector2(5, 0), Color.White);
        if (ShowDebug) DrawDebugScreen(gameTime);

        _spriteBatch.End();
    }

    // https://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
    private string AddSpacesToSentence(string text, bool preserveAcronyms = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }

        return newText.ToString();
    }

    private void DrawDebugScreen(GameTime gameTime)
    {
        string[] debugTexts =
        [
            $"",
            $"FPS: {Math.Round(1 / (float)gameTime.ElapsedGameTime.TotalSeconds, 1)} / {GameOptions.TargetFPS}",
            $"X: {LatibuleGame.Player.Position.X}",
            $"Y: {LatibuleGame.Player.Position.Y}",
            $"Z: {LatibuleGame.Player.Position.Z}",
            $"Velocity: {LatibuleGame.Player.Velocity}",
            $"Grounded: {LatibuleGame.Player.IsGrounded}",
        ];

        var startPosition = new Vector2(5, 0);
        DrawTextArray(debugTexts, startPosition, Color.White);

        // Draw text on the right side of the screen
        var rightSidePosition = new Vector2(_graphicsDevice.Viewport.Width - 5, 0);
        string[] rightAlignedTexts =
        [
            $"{Environment.UserName}@{Environment.MachineName}",
            $"OS: {Environment.OSVersion}",
            $"GC Used memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB",
            $"Time: {DateTime.Now:HH:mm:ss}",
            $"Resolution: {_graphicsDevice.Viewport.Width}x{_graphicsDevice.Viewport.Height}",
            $"{_graphicsDevice.Adapter.Description}",
        ];
        DrawTextArray(rightAlignedTexts, rightSidePosition, Color.White, true);
    }

    private void DrawCenterText(string text, Color color)
    {
        SpriteFontBase font = LatibuleGame.Fonts.GetFont(32);
        var fontOrigin = font.MeasureString(text) / 2;
        var position = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
        var scale = 2.0f;

        // draw text shadow
        _spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black, 0, fontOrigin, new Vector2(scale, scale));
        // draw text
        _spriteBatch.DrawString(font, text, position, color, 0, fontOrigin, new Vector2(scale, scale));
    }

    private void DrawText(string text, Vector2 position, Color color)
    {
        SpriteFontBase font = LatibuleGame.Fonts.GetFont(32);
        var fontOrigin = Vector2.Zero;
        // draw text shadow
        _spriteBatch.DrawString(font, text, position + new Vector2(2, 2), new Color(0, 0, 0, 0.5f), 0, fontOrigin);
        // draw text
        _spriteBatch.DrawString(font, text, position, color, 0, fontOrigin);
    }

    private void DrawTextArray(string[] texts, Vector2 startPosition, Color color, bool rightSide = false)
    {
        SpriteFontBase font = LatibuleGame.Fonts.GetFont(32);
        var fontOrigin = Vector2.Zero;

        // Draw the text
        for (var i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            if (rightSide) startPosition.X = _graphicsDevice.Viewport.Width - font.MeasureString(text).X - 5;

            var position = startPosition + new Vector2(0, i * font.LineHeight * 0.75f);
            // draw text shadow
            _spriteBatch.DrawString(font, text, position + new Vector2(2, 2), new Color(0, 0, 0, 0.5f), 0, fontOrigin);
            // draw text
            _spriteBatch.DrawString(font, text, position, color, 0, fontOrigin);
        }
    }

    private void DrawCrosshair()
    {
        // Get the center of the screen
        var centerX = _graphicsDevice.Viewport.Width / 2;
        var centerY = _graphicsDevice.Viewport.Height / 2;

        // Draw a horizontal line across the center
        _spriteBatch.Draw(_pixel,
            new Rectangle(centerX - CrosshairSize, centerY - CrosshairThickness / 2,
                CrosshairSize * 2, CrosshairThickness), _crosshairColor);

        // Draw a vertical line across the center
        _spriteBatch.Draw(_pixel,
            new Rectangle(centerX - CrosshairThickness / 2, centerY - CrosshairSize,
                CrosshairThickness, CrosshairSize * 2), _crosshairColor);
    }

    // Clean up resources when done
    public void Dispose()
    {
        _spriteBatch.Dispose();
        _pixel.Dispose();
        _graphicsDevice.Dispose();
    }
}