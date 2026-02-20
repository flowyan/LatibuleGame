using Latibule.Core.Audio;
using Latibule.Core.Rendering;
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
    private static readonly Dictionary<string, Texture> LoadedTextures = new();
    // public static readonly Dictionary<string, IntPtr> LoadedImGuiTextures = new();

    public static readonly Dictionary<string, WaveStream> LoadedSoundsList = [];

    private const float PitchMin = -0.2f;
    private const float PitchMax = 0.2f;

    public static void LoadAssets(GameWindow gameWindow)
    {
        LoadTextures();
        LoadSounds();
        // LoadShaders(content);

        // LatibuleGame.Fonts.AddFont(File.ReadAllBytes($"{Metadata.ASSETS_ROOT_DIRECTORY}/font/Jersey10.ttf"));
    }

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

    public static void PlaySound(SoundAsset soundAsset, float volume = 0.5f, bool randomPitch = true)
    {
        var outputDevice = new WaveOutEvent();
        var soundName = soundAsset.ToString().ToLower();
        var sound = LoadedSoundsList[soundName];
        // sound.Pitch = randomPitch ? (float)(new Random().NextDouble() * (PitchMax - PitchMin) + PitchMin) : 0;
        outputDevice.Init(sound);
        outputDevice.Volume = volume;
        outputDevice.Play();
    }

    public static void PlaySteamAudioSound(SoundAsset soundAsset, Vector3 soundPosition, float volume = 0.5f)
    {
        var soundName = soundAsset.ToString().ToLower();
        var stream = LoadedSoundsList[soundName];
        ISampleProvider sp = stream.ToSampleProvider();

        if (sp.WaveFormat.SampleRate != Core.SteamAudio.SamplingRate)
            sp = new WdlResamplingSampleProvider(sp, Core.SteamAudio.SamplingRate);

        // ensure mono for steam audio
        if (sp.WaveFormat.Channels == 2)
        {
            sp = new StereoToMonoSampleProvider(sp) { LeftVolume = 0.5f, RightVolume = 0.5f };
        }
        else if (sp.WaveFormat.Channels != 1)
        {
            stream.Dispose();
            throw new NotSupportedException($"Only mono/stereo supported. Got {sp.WaveFormat.Channels} channels.");
        }

        var spatial = new SteamAudioSampleProvider(sp, soundPosition, volume);

// Most reliable output path:
        IWaveProvider waveProvider = new SampleToWaveProvider16(spatial);

        var output = new WaveOutEvent();
        output.Init(waveProvider);
        output.Volume = 1.0f;
        output.Play();
    }
}