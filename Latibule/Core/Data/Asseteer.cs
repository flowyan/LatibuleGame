using Assimp;
using Assimp.Configs;
using FontStashSharp;
using Latibule.Core.Audio;
using Latibule.Core.Rendering;
using Latibule.Core.Rendering.Text;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using static Latibule.Core.Logger;

namespace Latibule.Core.Data;

/// <summary>
/// Manages game assets such as textures, sounds, and shaders.
/// </summary>
public static class Asseteer
{
    // keyed by shader path
    // public static readonly Dictionary<string, Effect> Shaders = [];

    // keyed by texture path
    private static readonly Dictionary<string, Texture> LoadedTextures = [];
    // public static readonly Dictionary<string, IntPtr> LoadedImGuiTextures = [];

    private static readonly Dictionary<string, WaveStream> LoadedSoundsList = [];

    private static readonly Dictionary<string, Scene> LoadedModels = [];

    public static FontStashRenderer FontRenderer = new();
    public static FontSystem FontSystem = new();
    public static int FontSize = 32;

    public static AssimpContext AssimpContext = new();

    private const float PitchMin = -0.2f;
    private const float PitchMax = 0.2f;

    public static void LoadAssets(GameWindow gameWindow)
    {
        LoadTextures();
        LoadSounds();
        LoadFonts();
        LoadModels();
        // LoadShaders(content);
    }

    // TODO: add support for subsubfolders so stuff like model_modelname_texture1 can work with TextureAsset
    private static void LoadTextures()
    {
        var textureDir = new DirectoryInfo($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}");
        if (!textureDir.Exists) throw new Exception($"Missing texture directory: {textureDir.FullName}");

        foreach (var file in textureDir.EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories))
        {
            var parentFolderName = file.Directory?.Name == Metadata.ASSETS_TEXTURE_PATH ? "" : $"{file.Directory?.Name}/";
            var textureName = $"{parentFolderName}{file.Name.Replace(file.Extension, "")}";
            try
            {
                var texture = new Texture($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}/{parentFolderName}{file.Name}");
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

    private static void LoadSounds()
    {
        var dirPath = $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SOUND_PATH}";
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

    public static Texture GetTexture(TextureAsset textureAsset)
    {
        // parse the texture path and return the texture
        var textureName = textureAsset.ToString().ToLower().Replace("_", "/");
        return LoadedTextures[textureName];
    }

    public static Texture[] GetTextures(TextureAsset[] textureAssets)
    {
        var textureNames = textureAssets.Select(x => x.ToString().ToLower().Replace("_", "/"));
        return LoadedTextures.Where(x => textureNames.Contains(x.Key)).Select(x => x.Value).ToArray();
    }

    private static void LoadFonts()
    {
        // Set up all the default font things
        FontRenderer = new FontStashRenderer();

        // TODO: go through the folder and add the fonts recursively
        FontSystem = new FontSystem(new FontSystemSettings
        {
            FontResolutionFactor = 4,
            KernelWidth = 2,
            KernelHeight = 2,
        });
        FontSystem.AddFont(File.ReadAllBytes(@"Assets/font/Jersey10.ttf"));
    }

    private static void LoadModels()
    {
        AssimpContext.SetConfig(new NormalSmoothingAngleConfig(66.0f));

        const PostProcessSteps flags = PostProcessSteps.Triangulate |
                                       PostProcessSteps.JoinIdenticalVertices |
                                       PostProcessSteps.CalculateTangentSpace |
                                       PostProcessSteps.GenerateNormals |
                                       PostProcessSteps.FlipUVs;

        const string dirPath = $"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_MODEL_PATH}";
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

    public static Scene GetModel(ModelAsset modelAsset)
    {
        var modelName = modelAsset.ToString().ToLower().Replace("_", "/");
        return LoadedModels[modelName];
    }

    public static void PlaySound(SoundAsset soundAsset, float volume = 0.5f, bool randomPitch = true)
    {
        var outputDevice = new WaveOutEvent();
        var soundName = soundAsset.ToString().ToLower();
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

    public static void PlaySteamAudioSound(SoundAsset soundAsset, Vector3 soundPosition, float volume = 0.5f)
    {
        var soundName = soundAsset.ToString().ToLower();
        var stream = LoadedSoundsList[soundName];
        var sp = stream.ToSampleProvider();

        if (sp.WaveFormat.SampleRate != SteamAudio.SamplingRate)
            sp = new WdlResamplingSampleProvider(sp, SteamAudio.SamplingRate);

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