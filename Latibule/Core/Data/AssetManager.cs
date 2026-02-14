using System.Drawing;
using FontStashSharp;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using static Latibule.Core.Logger;

namespace Latibule.Core.Data;

/// <summary>
/// Manages game assets such as textures, sounds, and shaders.
/// </summary>
public static class AssetManager
{
    // keyed by shader path
    public static readonly Dictionary<string, Effect> Shaders = [];

    // keyed by texture path
    private static readonly Dictionary<string, Texture2D> LoadedTextures = new();
    public static readonly Dictionary<string, IntPtr> LoadedImGuiTextures = new();

    public static readonly Dictionary<string, SoundEffect> LoadedSoundsList = [];

    private const float PitchMin = -0.2f;
    private const float PitchMax = 0.2f;

    public static void LoadAssets(ContentManager content, GraphicsDevice graphics)
    {
        LoadTextures(content);
        LoadSounds(content);
        // LoadShaders(content);

        LatibuleGame.Fonts.AddFont(File.ReadAllBytes($"{Metadata.ASSETS_ROOT_DIRECTORY}/font/Jersey10.ttf"));
    }

    private static void LoadTextures(ContentManager content)
    {
        var textureDir = new DirectoryInfo($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_TEXTURE_PATH}");
        if (!textureDir.Exists) throw new Exception($"Missing texture directory: {textureDir.FullName}");

        foreach (var file in textureDir.EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories))
        {
            var parentFolderName = file.Directory?.Name == Metadata.ASSETS_TEXTURE_PATH ? "" : $"{file.Directory?.Name}/";
            var textureName = $"{parentFolderName}{file.Name.Replace(file.Extension, "")}";
            try
            {
                var texture = content.Load<Texture2D>($"{Metadata.ASSETS_TEXTURE_PATH}/{parentFolderName}{file.Name}");
                LoadedTextures[textureName] = texture;
                LoadedImGuiTextures[textureName] = LatibuleGame.ImGuiRenderer.BindTexture(LoadedTextures[textureName]);
                LogInfo($"Loaded texture: {textureName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load sound: {textureName} ({file.Name}) - {e}");
            }
        }
    }

    private static void LoadSounds(ContentManager content)
    {
        var soundDir = new DirectoryInfo($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SOUND_PATH}");
        if (!soundDir.Exists) throw new Exception($"Missing sound directory: {soundDir.FullName}");

        foreach (var file in soundDir.EnumerateFiles())
        {
            var soundName = file.Name.Replace(file.Extension, "");
            try
            {
                var soundEffect = content.Load<SoundEffect>($"{Metadata.ASSETS_SOUND_PATH}/{file.Name}");
                LoadedSoundsList[soundName] = soundEffect;
                LogInfo($"Loaded sound: {soundName} ({file.Name})");
            }
            catch (Exception e)
            {
                LogError($"Failed to load sound: {soundName} ({file.Name}) - {e}");
            }
        }
    }

    private static void LoadShaders(ContentManager content)
    {
        var shaderDir = new DirectoryInfo($"{Metadata.ASSETS_ROOT_DIRECTORY}/{Metadata.ASSETS_SHADER_PATH}");
        if (!shaderDir.EnumerateFiles().Any())
        {
            LogWarning("No shaders found in the shader directory.");
            return;
        }

        foreach (var file in shaderDir.EnumerateFiles())
        {
            try
            {
                var shaderName = Path.GetFileNameWithoutExtension(file.Name).Replace(".xnb", "");
                var shader = content.Load<Effect>($"shader/{shaderName}");
                Shaders.Add(shaderName, shader);
                LogInfo($"Loaded shader: {shaderName}");
            }
            catch (Exception e)
            {
                LogError($"Failed to load shader: {file.Name}");
            }
        }
    }

    public static Texture2D GetTexture(TextureAsset textureAsset)
    {
        // parse the texture path and return the texture
        var textureName = textureAsset.ToString().ToLower().Replace("_", "/");
        return LoadedTextures[textureName];
    }

    public static void PlaySound(SoundAsset soundAsset, float volume = 0.5f, bool randomPitch = true)
    {
        var soundName = soundAsset.ToString().ToLower();
        var sound = LoadedSoundsList[soundName].CreateInstance();
        sound.Volume = volume;
        sound.Pitch = randomPitch ? (float)(new Random().NextDouble() * (PitchMax - PitchMin) + PitchMin) : 0;
        sound.Play();
    }

    public static void UnloadAssets()
    {
        foreach (var tex in LoadedTextures.Values) tex.Dispose();
        foreach (var shader in Shaders.Values) shader.Dispose();
        foreach (var sound in LoadedSoundsList.Values) sound.Dispose();
        LoadedTextures.Clear();
        Shaders.Clear();
        LoadedSoundsList.Clear();
        LoadedImGuiTextures.Clear();
    }
}