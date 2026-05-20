using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;

namespace Engine.Serialization;

public sealed class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0f;
        float y = 0f;

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected Vector2 object.");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector2(x, y);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected Vector2 property name.");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "x":
                case "X":
                    x = reader.GetSingle();
                    break;

                case "y":
                case "Y":
                    y = reader.GetSingle();
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end while reading Vector2.");
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }
}

public sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected Vector3 object.");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector3(x, y, z);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected Vector3 property name.");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "x":
                case "X":
                    x = reader.GetSingle();
                    break;

                case "y":
                case "Y":
                    y = reader.GetSingle();
                    break;

                case "z":
                case "Z":
                    z = reader.GetSingle();
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end while reading Vector3.");
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteEndObject();
    }
}

public sealed class Vector4JsonConverter : JsonConverter<Vector4>
{
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        float w = 0f;

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected Vector4 object.");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector4(x, y, z, w);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected Vector4 property name.");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName)
            {
                case "x":
                case "X":
                    x = reader.GetSingle();
                    break;

                case "y":
                case "Y":
                    y = reader.GetSingle();
                    break;

                case "z":
                case "Z":
                    z = reader.GetSingle();
                    break;

                case "w":
                case "W":
                    w = reader.GetSingle();
                    break;

                default:
                    reader.Skip();
                    break;
            }
        }

        throw new JsonException("Unexpected end while reading Vector4.");
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteNumber("z", value.Z);
        writer.WriteNumber("w", value.W);
        writer.WriteEndObject();
    }
}