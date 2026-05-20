using System.Text.Json;
using System.Text.Json.Serialization;

namespace Engine.Serialization;

public static class JsonSaveOptions
{
    public static readonly JsonSerializerOptions Default = Create();

    private static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new Vector2JsonConverter(), new Vector3JsonConverter(), new Vector4JsonConverter() }

            // PropertyNamingPolicy =  JsonNamingPolicy.CamelCase
        };

        return options;
    }
}