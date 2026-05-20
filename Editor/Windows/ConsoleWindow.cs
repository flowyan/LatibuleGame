using Editor.Core.Types;
using Engine.Core;
using Engine.Core.Types;
using ImGuiNET;
using OpenTK.Windowing.Common;
using Vector4 = System.Numerics.Vector4;

namespace Editor.Windows;

public sealed class ConsoleWindow : IEditorWindow, IDisposable
{
    public string Title => "Console";
    public EditorWindowSlot Slot => EditorWindowSlot.BottomRight;

    public void Render(FrameEventArgs e)
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0f, 0f, 0f, 1f));
        ImGui.BeginChild("##editor-console-messages", new System.Numerics.Vector2(0, -ImGui.GetFrameHeightWithSpacing() - 4f));

        foreach (var message in DevConsole.Messages)
        {
            var content = message.Type == ConsoleMessageType.CommandOutput ? message.Content : $"{message}";
            ImGui.TextColored(ToColor(message), content);
        }

        if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
            ImGui.SetScrollHereY(1f);

        ImGui.EndChild();
        ImGui.PopStyleColor();

        ImGui.SetNextItemWidth(-1f);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new System.Numerics.Vector4(0f, 0f, 0f, 1f));
        if (ImGui.InputTextWithHint("##editor-console-input", "Enter command (help, clear, echo)", ref DevConsole.CurrentCommand, 256, ImGuiInputTextFlags.EnterReturnsTrue))
            DevConsole.ExecuteCommand(DevConsole.CurrentCommand);
        ImGui.PopStyleColor();
    }

    public void Update(FrameEventArgs e)
    {
    }

    public void Dispose()
    {
    }

    private static Vector4 ToColor(ConsoleMessage message)
    {
        return message.Type switch
        {
            ConsoleMessageType.Error => new Vector4(1f, 0.35f, 0.35f, 1f),
            ConsoleMessageType.Warning => new Vector4(1f, 0.9f, 0.35f, 1f),
            ConsoleMessageType.Debug => new Vector4(0.95f, 0.45f, 1f, 1f),
            ConsoleMessageType.Info => new Vector4(0.85f, 0.9f, 1f, 1f),
            ConsoleMessageType.CommandOutput => message.Color,
            _ => new Vector4(1f, 1f, 1f, 1f)
        };
    }
}