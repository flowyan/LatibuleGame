using Editor.Core.Types;
using Editor.Windows.Menubar;
using ImGuiNET;
using OpenTK.Windowing.Common;
using Vector2 = System.Numerics.Vector2;

namespace Editor.Core;

public sealed class EditorWindowSystem(List<IEditorWindow> windows) : IDisposable
{
    public void Update(FrameEventArgs e)
    {
        foreach (var window in windows)
        {
            window.Update(e);
        }
    }

    public void Render(FrameEventArgs e)
    {
        var rects = CalculateGridRects();

        MainMenuBar();

        foreach (var window in windows)
        {
            var (position, size) = rects[window.Slot];
            ImGui.SetNextWindowPos(position, ImGuiCond.Always);
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);
            ImGui.Begin(window.Title ?? "noname",
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                (window.Title == null ? ImGuiWindowFlags.NoTitleBar : ImGuiWindowFlags.None)
            );

            window.Render(e);
            ImGui.End();
        }
    }

    public void Dispose()
    {
        foreach (var window in windows)
            if (window is IDisposable disposableWindow)
                disposableWindow.Dispose();
    }

    private static void MainMenuBar()
    {
        if (!ImGui.BeginMainMenuBar()) return;

        FileMenubar.Render();

        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Todo"))
            {
            }

            ImGui.EndMenu();
        }

        ImGui.EndMainMenuBar();
    }

    private static Dictionary<EditorWindowSlot, (Vector2 Position, Vector2 Size)> CalculateGridRects()
    {
        var mainViewport = ImGui.GetMainViewport();
        var workPos = mainViewport.WorkPos;
        var workSize = mainViewport.WorkSize;

        var topHeight = workSize.Y * EditorOptions.TopSectionRatio;
        var bottomHeight = workSize.Y - topHeight - EditorOptions.GridGap;

        var leftWidth = workSize.X * EditorOptions.LeftPanelRatio;
        var rightWidth = workSize.X * EditorOptions.RightPanelRatio;
        var centerWidth = workSize.X - leftWidth - rightWidth - (EditorOptions.GridGap * 2f);

        return new Dictionary<EditorWindowSlot, (Vector2 Position, Vector2 Size)>
        {
            [EditorWindowSlot.TopLeft] = (workPos, new Vector2(leftWidth, topHeight)),
            [EditorWindowSlot.TopCenter] = (new Vector2(workPos.X + leftWidth + EditorOptions.GridGap, workPos.Y), new Vector2(centerWidth, topHeight)),
            [EditorWindowSlot.TopRight] = (new Vector2(workPos.X + leftWidth + centerWidth + (EditorOptions.GridGap * 2f), workPos.Y), new Vector2(rightWidth, topHeight)),
            [EditorWindowSlot.BottomRight] = (new Vector2(workPos.X + leftWidth + centerWidth + (EditorOptions.GridGap * 2f), workPos.Y + topHeight + EditorOptions.GridGap), new Vector2(rightWidth, bottomHeight)),
            [EditorWindowSlot.BottomFullWidth] = (workPos + new Vector2(0, topHeight + EditorOptions.GridGap), new Vector2(workSize.X, bottomHeight))
        };
    }
}