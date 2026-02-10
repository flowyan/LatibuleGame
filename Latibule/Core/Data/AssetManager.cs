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
    public static readonly Dictionary<string, Texture2D> LoadedTextures = new();
    public static readonly Dictionary<string, IntPtr> LoadedImGuiTextures = new();

    public static readonly Dictionary<string, SoundEffect> LoadedSoundsList = [];

    private const float PitchMin = -0.2f;
    private const float PitchMax = 0.2f;

    public static void LoadAssets(ContentManager content, GraphicsDevice graphics)
    {
        LoadTextures(content, graphics);
        LoadSounds(content);
        // LoadShaders(content);

        LatibuleGame.Fonts.AddFont(File.ReadAllBytes($"{Metadata.ASSETS_ROOT_DIRECTORY}/font/Jersey10.ttf"));
    }

    private static void LoadTextures(ContentManager content, GraphicsDevice graphics)
    {
        HashSet<string> textures = [];

        foreach (var mat in Enum.GetValues<Material>())
        {
            textures.Add($"material/{mat.ToString().ToLower()}");
        }

        // Always ensure we have a fallback
        var missingTexPath = "missing";
        Texture2D missingTex;
        try
        {
            missingTex = content.Load<Texture2D>($"{Metadata.ASSETS_TEXTURE_PATH}/{missingTexPath}");
        }
        catch
        {
            LogError($"Critical: Missing fallback texture '{missingTexPath}', creating red placeholder.");
            missingTex = new Texture2D(graphics, 16, 16);
            var colorData = Enumerable.Repeat(Color.Purple, 16 * 16).ToArray();
            missingTex.SetData(colorData);
        }

        foreach (var key in textures)
        {
            try
            {
                LoadedTextures[key] = content.Load<Texture2D>($"{Metadata.ASSETS_TEXTURE_PATH}/{key}");
                LoadedImGuiTextures[key] = LatibuleGame.ImGuiRenderer.BindTexture(LoadedTextures[key]);
                LogInfo($"Loaded texture: {key}");
            }
            catch
            {
                LogWarning($"Missing texture: {key}");
                LoadedTextures[key] = content.Load<Texture2D>($"{Metadata.ASSETS_TEXTURE_PATH}/missing");
                LoadedImGuiTextures[key] = LatibuleGame.ImGuiRenderer.BindTexture(LoadedTextures[key]);
            }
        }

        // If for some reason no paths were found, at least insert the fallback
        if (LoadedTextures.Count == 0)
        {
            LogWarning("Warning: No block textures found in JSON, inserting fallback.");
            LoadedTextures[missingTexPath] = missingTex;
            LoadedImGuiTextures[missingTexPath] = LatibuleGame.ImGuiRenderer.BindTexture(missingTex);
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

    public static void PlaySound(string soundName, float volume = 0.5f, bool randomPitch = true)
    {
        var sound = LoadedSoundsList[soundName].CreateInstance();
        sound.Volume = volume;
        sound.Pitch = randomPitch ? (float)(new Random().NextDouble() * (PitchMax - PitchMin) + PitchMin) : 0;
        sound.Play();
    }
}