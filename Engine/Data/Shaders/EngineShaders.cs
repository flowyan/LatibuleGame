namespace Engine.Data.Shaders;

public abstract class EngineShaders
{
    public const EngineShader DefaultShader = EngineShader.mesh;

    public const EngineShader Mesh = EngineShader.mesh;
    public const EngineShader DebugUi = EngineShader.debugui;
    public const EngineShader Text = EngineShader.text;
    public const EngineShader Skybox = EngineShader.skybox;

    public enum EngineShader
    {
        mesh,
        debugui,
        text,
        skybox
    }
}