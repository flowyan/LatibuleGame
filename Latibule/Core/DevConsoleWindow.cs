using System.Numerics;
using Engine.Core;
using Engine.Core.Types;
using ImGuiNET;
using OpenTK.Windowing.Common;

namespace Latibule.Core;

public class DevConsoleWindow : IGuiScreen
{
    public void Initialize()
    {
    }

    public void OnRenderFrame(FrameEventArgs args)
    {
        ImGui.Begin("Dev Console", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings);

        var sizeX = EngineStates.GameWindow.Size.X - 500;
        var sizeY = EngineStates.GameWindow.Size.Y - 200;
        var x = ImGui.GetIO().DisplaySize.X / 2 - sizeX / 2;
        var y = ImGui.GetIO().DisplaySize.Y / 2 - sizeY / 2;

        ImGui.SetWindowPos(new Vector2(x, y), ImGuiCond.Appearing);
        ImGui.SetWindowSize(new Vector2(sizeX, sizeY), ImGuiCond.Once);

        ImGui.BeginChild("##messages", new Vector2(0, -ImGui.GetFrameHeightWithSpacing()));

        var messages = string.Join("\n", DevConsole.Messages);
        var textSize = ImGui.CalcTextSize(messages);
        textSize.X = ImGui.GetWindowWidth();
        textSize.Y += 5;

        ImGui.PushID(0);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));

        ImGui.InputTextMultiline(
            "",
            ref messages,
            (uint)messages.Length + 1,
            textSize,
            ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.NoHorizontalScroll
        );

        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.PopID();

        // Autoscroll
        if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY()) ImGui.SetScrollHereY(1);

        ImGui.EndChild();
        if (ImGui.IsKeyPressed(ImGuiKey.Enter)) ImGui.SetKeyboardFocusHere();
        if (ImGui.IsKeyPressed(ImGuiKey.GraveAccent)) ImGui.SetKeyboardFocusHere();

        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 0, 1));
        // max width
        ImGui.SetNextItemWidth(-1);
        var input = ImGui.InputTextWithHint("##input", "Enter command here", ref DevConsole.CurrentCommand, 256,
            ImGuiInputTextFlags.EnterReturnsTrue);
        if (input && DevConsole.CurrentCommand.Length > 0) DevConsole.ExecuteCommand(DevConsole.CurrentCommand);
        ImGui.PopStyleColor();

        ImGui.End();
    }
}