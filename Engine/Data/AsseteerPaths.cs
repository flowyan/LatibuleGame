namespace Engine.Data;

public class AsseteerPaths(
    string rootDirectory,
    string textureDirectory,
    string soundDirectory,
    string shaderDirectory,
    string fontDirectory,
    string modelDirectory
)
{
    public string RootDirectory { get; } = rootDirectory;
    public string TextureDirectory { get; } = textureDirectory;
    public string SoundDirectory { get; } = soundDirectory;
    public string ShaderDirectory { get; } = shaderDirectory;
    public string FontDirectory { get; } = fontDirectory;
    public string ModelDirectory { get; } = modelDirectory;
}