namespace Engine.Data.Shaders;

public abstract class EngineShaders
{
    public const EngineShader DefaultShader = EngineShader.mesh;

    public const EngineShader Mesh = EngineShader.mesh;
    public const EngineShader DebugUi = EngineShader.debugui;
    public const EngineShader Text = EngineShader.text;

    public enum EngineShader
    {
        mesh,
        debugui,
        text
    }
}