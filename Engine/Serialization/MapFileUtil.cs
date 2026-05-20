using System.Text.Json;
using System.Text.Json.Serialization;
using Engine.Core;
using Engine.Rendering;

namespace Engine.Serialization;

public static class MapFileUtil
{
    private const string RootFolderName = "Latibule";
    private const string MapFolderName = "maps";

    private static readonly string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public static void SaveMapToFile(string mapName = "map")
    {
        CheckAndCreateDirectories();
        var location = $"{documentsFolder}/{RootFolderName}/maps/{mapName}.json";
        Logger.LogInfo($"Writing map file to {location}");

        var jsonString = JsonSerializer.Serialize(LatibuleEngine.Map, JsonSaveOptions.Default);

        File.WriteAllText(location, jsonString);
        Logger.LogInfo($"Saved map file to {location}");
    }

    public static GameMap? LoadMapFromFile(string? path = null)
    {
        if (path == null) path = $"{documentsFolder}/{RootFolderName}/maps/map.json";
        if (!File.Exists(path))
        {
            Logger.LogError($"Map file not found at {path}");
            return null;
        }

        Logger.LogInfo($"Loading map from {path}");
        var deserializedMap = JsonSerializer.Deserialize<GameMap>(File.ReadAllText(path), JsonSaveOptions.Default);
        Logger.LogInfo($"Loaded map from {path}");

        Logger.LogInfo($"Deserialized map has:\n" +
                       $"{deserializedMap?.Objects.Count} objects\n" +
                       $"{deserializedMap?.Lights.Length} lights\n" +
                       $"{deserializedMap?.Objects.Count(o => o.PhysicsBodyID != null)} physics bodies");

        return deserializedMap;
    }

    private static void CheckAndCreateDirectories()
    {
        if (!Directory.Exists($"{documentsFolder}/{RootFolderName}"))
            Directory.CreateDirectory($"{documentsFolder}/{RootFolderName}");

        if (!Directory.Exists($"{documentsFolder}/{RootFolderName}/{MapFolderName}"))
            Directory.CreateDirectory($"{documentsFolder}/{RootFolderName}/{MapFolderName}");
    }
}