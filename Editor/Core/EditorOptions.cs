namespace Editor.Core;

public static class EditorOptions
{
    public static int TargetFPS = 60;
    public static float GridGap = 0f;
    public static float LeftPanelRatio = 0.2f;
    public static float RightPanelRatio = 0.25f;
    public static float TopSectionRatio = 0.65f;
    
    // Used inside MapSettingsWindow.cs
    public static bool RenderBoundingBoxes = true;
    public static bool UpdateMap = false;
}