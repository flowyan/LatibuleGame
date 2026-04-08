using Assimp;
using Assimp.Configs;
using Engine.Audio;
using Engine.Core;
using Engine.Rendering;
using Engine.Rendering.Text;
using FontStashSharp;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
    private static readonly Dictionary<string, WaveStream> LoadedSoundsList = [];
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
            var textureName = $"{parentFolderName}{file.Name.Replace(file.Extension, "")}";
            try
            {
                var texture = new Texture($"{paths.RootDirectory}/{paths.TextureDirectory}/{parentFolderName}{file.Name}");
                LoadedTextures[textureName] = texture;
                // LoadedImGuiTextures[textureName] = LatibuleGame.ImGuiRenderer.BindTexture(LoadedTextures[textureName]);
                LogInfo($"Loaded texture: {textureName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load texture: {textureName} ({file.Name}) - {e}");
            }
        }
    }

    public static Texture GetTexture(dynamic textureAsset)
    {
        // parse the texture path and return the texture
        var textureName = textureAsset.ToString().Replace("_", "/");
        try
        {
            return LoadedTextures[textureName];
        }
        catch (Exception e)
        {
            LogError(e.Message);
            return GetTexture("missing");
        }
    }

    public static Texture[] GetTextures(dynamic[] textureAssets)
    {
        var textureNames = textureAssets.Select(x => x.ToString().Replace("_", "/"));
        return LoadedTextures.Where(x => textureNames.Contains(x.Key)).Select(x => x.Value).ToArray();
    }

    private void LoadSounds()
    {
        var dirPath = $"{paths.RootDirectory}/{paths.SoundDirectory}";
        var soundDir = new DirectoryInfo(dirPath);
        if (!soundDir.Exists) throw new Exception($"Missing sound directory: {soundDir.FullName}");

        foreach (var file in soundDir.EnumerateFiles())
        {
            var extension = file.Extension;
            var soundName = file.Name.Replace(extension, "");
            var soundPath = $"{dirPath}/{file.Name}";
            try
            {
                // todo: dont load readers, load smthing else cuz issues with looping and playback
                LoadedSoundsList[soundName] = extension switch
                {
                    ".ogg" => new VorbisWaveReader(soundPath),
                    _ => new AudioFileReader(soundPath)
                };

                LogInfo($"Loaded sound: {soundName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load sound: {soundName} ({file.Name}) - {e}");
            }
        }
    }

    private void LoadShaders()
    {
        var shaderDir = new DirectoryInfo($"{paths.RootDirectory}/{paths.ShaderDirectory}");
        if (!shaderDir.Exists) throw new Exception($"Missing shader directory: {shaderDir.FullName}");

        foreach (var file in shaderDir.EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories))
        {
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

    public static Shader GetShader(dynamic shaderAsset)
    {
        var shaderName = shaderAsset.ToString().Replace("_", "/");
        var resolvedShaderName = ResolveShaderNameForContext(shaderName);
        return LoadedShaders[resolvedShaderName];
    }

    private static string ResolveShaderNameForContext(string shaderName)
    {
        // In editor mode, force all mesh-shader lookups to use the editor's unlit mesh shader.
        if (!EngineWindow.IS_EDITOR) return shaderName;
        if (shaderName != "mesh/shader") return shaderName;

        const string editorMeshShaderName = "editormesh/shader";
        if (LoadedShaders.ContainsKey(editorMeshShaderName)) return editorMeshShaderName;

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

    public static void PlaySound(dynamic soundAsset, float volume = 0.5f, bool randomPitch = true)
    {
        var outputDevice = new WaveOutEvent();
        var soundName = soundAsset.ToString();
        var sound = LoadedSoundsList[soundName];
        // var loopStream = new LoopStream(sound);
        // sound.Pitch = randomPitch ? (float)(new Random().NextDouble() * (PitchMax - PitchMin) + PitchMin) : 0;

        outputDevice.PlaybackStopped += (sender, args) =>
        {
            outputDevice.Dispose();
            sound.Position = 0;
            outputDevice = new WaveOutEvent();
        };

        outputDevice.Init(sound);
        outputDevice.Volume = volume;
        outputDevice.Play();
    }

    public static void PlaySteamAudioSound(dynamic soundAsset, Vector3 soundPosition, float volume = 0.5f)
    {
        var soundName = soundAsset.ToString();
        var stream = LoadedSoundsList[soundName];
        var sp = stream.ToSampleProvider();

        if (sp.WaveFormat.SampleRate != Audio.SteamAudio.SamplingRate)
            sp = new WdlResamplingSampleProvider(sp, Audio.SteamAudio.SamplingRate);

        var spatial = new SteamAudioSampleProvider(sp, soundPosition, volume);

        // Most reliable output path:
        IWaveProvider waveProvider = new SampleToWaveProvider24(spatial);

        var device = new NAudio.CoreAudioApi.MMDeviceEnumerator()
            .GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

        // TODO: switch to something crossplatform, probably OpenAL teehee
        var outDevice = new WasapiOut(device,
            NAudio.CoreAudioApi.AudioClientShareMode.Shared,
            true,
            latency: 30);

        outDevice.Init(waveProvider);
        outDevice.Volume = volume;
        outDevice.Play();
    }
}