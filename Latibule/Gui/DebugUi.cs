using System.Text;
using FontStashSharp;
using Latibule.Core;
using Latibule.Core.Data;
using Latibule.Core.Rendering;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace Latibule.Gui;

public class DebugUi
{
    public bool ShowDebug = Environment.GetEnvironmentVariable("debug") == "true";

    public void OnRenderFrame(FrameEventArgs args)
    {
        if (!GameStates.ShowHud) return;
        DrawText($"{Metadata.GAME_NAME} {Metadata.GAME_VERSION}", new Vector2(5, 0), FSColor.White);
        if (ShowDebug) DrawDebugScreen(args);
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

    private void DrawDebugScreen(FrameEventArgs args)
    {
        var gameWindow = GameStates.GameWindow;
        string[] debugTexts =
        [
            $"",
            $"FPS: {Math.Round(1 / (float)args.Time, 1)} / {GameOptions.TargetFPS}",
            $"X: {LatibuleGame.Player.Transform.Position.X}",
            $"Y: {LatibuleGame.Player.Transform.Position.Y}",
            $"Z: {LatibuleGame.Player.Transform.Position.Z}",
            $"Velocity: {LatibuleGame.Player.Velocity}",
            $"Grounded: {LatibuleGame.Player.IsGrounded}",
        ];

        var startPosition = new Vector2(5, 0);
        DrawTextArray(debugTexts, startPosition, FSColor.White);

        // Draw text on the right side of the screen
        var rightSidePosition = new Vector2(gameWindow.ClientSize.X - 5, 0);
        string[] rightAlignedTexts =
        [
            $"{Environment.UserName}@{Environment.MachineName}",
            $"OS: {Environment.OSVersion}",
            $"GC Used memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB",
            $"Time: {DateTime.Now:HH:mm:ss}",
            $"Resolution: {gameWindow.ClientSize.X}x{gameWindow.ClientSize.Y}",
            $"{gameWindow.API.ToString()} {gameWindow.APIVersion}",
        ];
        DrawTextArray(rightAlignedTexts, rightSidePosition, FSColor.White, true);
    }

    private void DrawCenterText(string text, FSColor color)
    {
        // var fontOrigin = font.MeasureString(text) / 2;
        // var position = new Vector2(GameStates.GameWindow.ClientSize.X / 2, GameStates.GameWindow.ClientSize.Y / 2);
        // var scale = 2.0f;
        //
        // // draw text shadow
        // _spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black, 0, fontOrigin, new Vector2(scale, scale));
        // // draw text
        // _spriteBatch.DrawString(font, text, position, color, 0, fontOrigin, new Vector2(scale, scale));
    }

    private void DrawText(string text, Vector2 position, FSColor color)
    {
        var font = Asseteer.FontSystem.GetFont(Asseteer.FontSize);
        // draw text
        var _renderer = new UiTextRenderer(text, position, 1, color);
        _renderer.Render();
    }

    private void DrawTextArray(string[] texts, Vector2 startPosition, FSColor color, bool rightSide = false)
    {
        // Draw the text
        for (var i = 0; i < texts.Length; i++)
        {
            var text = texts[i];
            var font = Asseteer.FontSystem.GetFont(Asseteer.FontSize);
            if (rightSide) startPosition.X = (GameStates.GameWindow.ClientSize.X - font.MeasureString(text).X) - 5;

            var position = startPosition + new Vector2(0, i * font.LineHeight * 0.75f);
            // draw text
            var _renderer = new UiTextRenderer(text, position, 1);
            _renderer.Render();
        }
    }
}