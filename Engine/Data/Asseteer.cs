using System.Reflection;
using Assimp;
using Assimp.Configs;
using Engine.Audio;
using Engine.Core;
using Engine.Data.Shaders;
using Engine.Rendering;
using Engine.Rendering.Text;
using FontStashSharp;
using OpenTK.Mathematics;
using static Engine.Core.Logger;
using Vector2 = System.Numerics.Vector2;

namespace Engine.Data;

/// <summary>
/// Manages game assets such as textures, sounds, and shaders.
/// </summary>
public class Asseteer(AsseteerPaths paths)
{
    public static bool Loaded { get; private set; } = false;

    // keyed by texture path
    private static readonly Dictionary<string, Texture> LoadedTextures = [];
    // private static readonly Dictionary<string, Func<WaveStream>> LoadedSoundsList = [];
    private static readonly Dictionary<string, Shader> LoadedShaders = [];
    private static readonly Dictionary<string, Scene> LoadedModels = [];

    public static FontStashRenderer FontRenderer = null!;
    public static FontSystem FontSystem = null!;
    public static int FontSize = 32;

    public static AssimpContext AssimpContext = new();

    private const float PitchMin = -0.2f;
    private const float PitchMax = 0.2f;

    public void LoadEssentialAssets()
    {
        LoadShaders();
        LoadTextures();
        LoadFonts();
    }

    /**
     * Loads all game assets. This should be called once at the start of the game.
     */
    public void LoadAssets()
    {
        LoadEssentialAssets();
        FontRenderer.Begin();
        // After loading fonts we can show a brief loading
        var font = FontSystem.GetFont(FontSize);
        font.DrawText(
            FontRenderer,
            "LOADING LOL",
            new Vector2(10, 10),
            FSColor.Red,
            0,
            Vector2.Zero,
            Vector2.One
        );

        LoadSounds();
        LoadModels();

        // Play a tada! when done :)
        // PlaySound(BuiltinSoundAsset.tada, volume: 0.25f, randomPitch: false);
        Loaded = true;
        FontRenderer.End();
    }

    // TODO: add support for subsubfolders so stuff like model_modelname_texture1 can work with TextureAsset
    private void LoadTextures()
    {
        var textureDir = new DirectoryInfo($"{paths.RootDirectory}/{paths.TextureDirectory}");
        if (!textureDir.Exists) throw new Exception($"Missing texture directory: {textureDir.FullName}");

        foreach (var file in textureDir.EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories))
        {
            var parentFolderName = file.Directory?.Name == paths.TextureDirectory ? "" : $"{file.Directory?.Name}/";
            var texturePath = $"{parentFolderName}{file.Name.Replace(file.Extension, "")}";
            try
            {
                var texture = new Texture($"{paths.RootDirectory}/{paths.TextureDirectory}/{parentFolderName}{file.Name}");
                LoadedTextures[texturePath] = texture;
                // LoadedImGuiTextures[textureName] = LatibuleGame.ImGuiRenderer.BindTexture(LoadedTextures[textureName]);
                LogInfo($"Loaded texture: {texturePath} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load texture: {texturePath} ({file.Name}) - {e}");
            }
        }
    }

    public static Texture GetTexture<TEnum>(TEnum textureAsset) where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        var enumTypeName = enumType.Name.ToLowerInvariant(); // Dev -> dev
        var assetName = textureAsset.ToString(); // dev_measuregeneric01

        var texturePath = $"{enumTypeName}/{assetName}";

        if (LoadedTextures.TryGetValue(texturePath, out var texture))
            return texture;

        LogError($"Texture '{texturePath}' was not found.");
        return GetMissingTexture();
    }

    private static Texture GetMissingTexture()
    {
        return LoadedTextures.TryGetValue("missing", out var missing) ? missing : throw new InvalidOperationException("Missing texture was not loaded.");
    }

    public static Texture[] GetTextures<TEnum>(params TEnum[] textureAssets)
        where TEnum : struct, Enum
    {
        return textureAssets
            .Select(textureAsset =>
            {
                var enumType = typeof(TEnum);
                var folder = enumType.Name.ToLowerInvariant();
                var file = textureAsset.ToString();

                var path = $"{folder}/{file}";

                if (LoadedTextures.TryGetValue(path, out var texture))
                    return texture;

                LogError($"Texture '{path}' was not found.");
                return GetMissingTexture();
            })
            .ToArray();
    }

    private void LoadSounds()
    {
        var dirPath = Path.Combine(paths.RootDirectory, paths.SoundDirectory);
        var soundDir = new DirectoryInfo(dirPath);

        if (!soundDir.Exists)
            throw new DirectoryNotFoundException($"Missing sound directory: {soundDir.FullName}");

        foreach (var file in soundDir.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var extension = file.Extension.ToLowerInvariant();

            if (extension is not ".ogg" and not ".wav" and not ".mp3")
                continue;

            var relativePath = Path.GetRelativePath(dirPath, file.FullName);
            var soundName = Path.ChangeExtension(relativePath, null)!
                .Replace('\\', '/')
                .ToLowerInvariant();

            try
            {
                // LoadedSoundsList[soundName] = () => CreateSoundStream(file.FullName, extension);
                LogInfo($"Loaded sound: {soundName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to register sound: {soundName} ({file.Name}) - {e}");
            }
        }
    }

    // private static WaveStream CreateSoundStream(string filePath, string extension)
    // {
    //     return extension switch
    //     {
    //         ".ogg" => new VorbisWaveReader(filePath),
    //         ".wav" => new AudioFileReader(filePath),
    //         ".mp3" => new AudioFileReader(filePath),
    //         _ => throw new NotSupportedException($"Unsupported sound format: {extension}")
    //     };
    // }

    private void LoadShaders()
    {
        var shaderDir = new DirectoryInfo($"{paths.RootDirectory}/{paths.ShaderDirectory}");
        if (!shaderDir.Exists) throw new Exception($"Missing shader directory: {shaderDir.FullName}");

        foreach (var file in shaderDir.EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories))
        {
            // Only process .vert files to avoid loading the same shader twice (once from .vert, once from .frag)
            if (file.Extension != ".vert") continue;

            var parentFolderName = file.Directory?.Name == paths.ShaderDirectory ? "" : $"{file.Directory?.Name}/";
            var shaderName = $"{parentFolderName}{file.Name.Replace(file.Extension, "")}";
            try
            {
                var shader = new Shader(
                    $"{paths.RootDirectory}/{paths.ShaderDirectory}/{shaderName}.vert",
                    $"{paths.RootDirectory}/{paths.ShaderDirectory}/{shaderName}.frag"
                );
                LoadedShaders[shaderName] = shader;
                LogInfo($"Loaded shader: {shaderName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load shader: {shaderName} ({file.Name}) - {e}");
            }
        }
    }

    public static Shader GetShader(EngineShaders.EngineShader shaderAsset)
    {
        var shaderName = $"{shaderAsset.ToString().ToLowerInvariant()}/shader";
        var resolvedShaderName = ResolveShaderNameForContext(shaderName);

        if (LoadedShaders.TryGetValue(resolvedShaderName, out var shader))
            return shader;

        throw new KeyNotFoundException($"Shader '{resolvedShaderName}' was not found.");
    }

    private static string ResolveShaderNameForContext(string shaderName)
    {
        if (!EngineWindow.IS_EDITOR || shaderName != "mesh/shader") return shaderName;

        const string editorMeshShaderName = "editormesh/shader";

        if (LoadedShaders.ContainsKey(editorMeshShaderName))
            return editorMeshShaderName;

        LogWarning($"Editor shader alias missing: '{editorMeshShaderName}'. Falling back to '{shaderName}'.");
        return shaderName;
    }

    private static void LoadFonts()
    {
        // Set up all the default font things
        FontRenderer = new FontStashRenderer();

        // TODO: go through the folder and add the fonts recursively
        FontSystem = new FontSystem(new FontSystemSettings
        {
            FontResolutionFactor = 8,
            KernelWidth = 1,
            KernelHeight = 1,
        });
        FontSystem.AddFont(File.ReadAllBytes(@"Assets/font/Jersey10.ttf"));
    }

    private void LoadModels()
    {
        AssimpContext.SetConfig(new NormalSmoothingAngleConfig(66.0f));

        const PostProcessSteps flags = PostProcessSteps.Triangulate |
                                       PostProcessSteps.JoinIdenticalVertices |
                                       PostProcessSteps.CalculateTangentSpace |
                                       PostProcessSteps.GenerateNormals |
                                       PostProcessSteps.FlipUVs;

        var dirPath = $"{paths.RootDirectory}/{paths.ModelDirectory}";
        var modelDir = new DirectoryInfo(dirPath);
        if (!modelDir.Exists) throw new Exception($"Missing model directory: {modelDir.FullName}");

        foreach (var file in modelDir.EnumerateFiles())
        {
            var extension = file.Extension;
            var modelName = file.Name.Replace(extension, "");
            var modelPath = $"{dirPath}/{file.Name}";
            try
            {
                LoadedModels[modelName] = AssimpContext.ImportFile(modelPath, flags);

                LogInfo($"Loaded model: {modelName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load model: {modelName} ({file.Name}) - {e}");
            }
        }
    }

    public static Scene GetModel(dynamic modelAsset)
    {
        var modelName = modelAsset.ToString().Replace("_", "/");
        return LoadedModels[modelName];
    }

    public static void PlaySound<TEnum>(TEnum soundAsset, float volume = 0.5f, bool randomPitch = true)
        where TEnum : struct, Enum
    {
        var folder = typeof(TEnum).Name.ToLowerInvariant();
        var file = soundAsset.ToString();
        var soundName = $"{folder}/{file}";

        // if (!LoadedSoundsList.TryGetValue(soundName, out var soundFactory))
        // {
        //     LogError($"Sound '{soundName}' was not found.");
        //     return;
        // }
        //
        // var sound = soundFactory();
        // var outputDevice = new WaveOutEvent();
        //
        // outputDevice.PlaybackStopped += (_, _) =>
        // {
        //     outputDevice.Dispose();
        //     sound.Dispose();
        // };
        //
        // outputDevice.Init(sound);
        // outputDevice.Volume = volume;
        // outputDevice.Play();
    }

    // public static void PlaySteamAudioSound<TEnum>(TEnum soundAsset, Vector3 soundPosition, float volume = 0.5f)
    //     where TEnum : struct, Enum
    // {
    //     var folder = typeof(TEnum).Name.ToLowerInvariant();
    //     var file = soundAsset.ToString();
    //     var soundName = $"{folder}/{file}";
    //
    //     if (!LoadedSoundsList.TryGetValue(soundName, out var soundFactory))
    //     {
    //         LogError($"Sound '{soundName}' was not found.");
    //         return;
    //     }
    //
    //     var stream = soundFactory();
    //     var sp = stream.ToSampleProvider();
    //
    //     if (sp.WaveFormat.SampleRate != Audio.SteamAudio.SamplingRate)
    //         sp = new WdlResamplingSampleProvider(sp, Audio.SteamAudio.SamplingRate);
    //
    //     var spatial = new SteamAudioSampleProvider(sp, soundPosition, volume);
    //     IWaveProvider waveProvider = new SampleToWaveProvider24(spatial);
    //
    //     var device = new NAudio.CoreAudioApi.MMDeviceEnumerator()
    //         .GetDefaultAudioEndpoint(
    //             NAudio.CoreAudioApi.DataFlow.Render,
    //             NAudio.CoreAudioApi.Role.Multimedia);
    //
    //     var outDevice = new WasapiOut(
    //         device,
    //         NAudio.CoreAudioApi.AudioClientShareMode.Shared,
    //         true,
    //         latency: 30);
    //
    //     outDevice.PlaybackStopped += (_, _) =>
    //     {
    //         outDevice.Dispose();
    //         stream.Dispose();
    //     };
    //
    //     outDevice.Init(waveProvider);
    //     outDevice.Volume = volume;
    //     outDevice.Play();
    // }
}