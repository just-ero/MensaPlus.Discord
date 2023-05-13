using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using OpenMensa.Models;

namespace OpenMensa.Serialization;

public sealed class CoordinatesConverter : JsonConverter<Coordinates>
{
    public override Coordinates Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            _ = $"Expected start of array, but got '{reader.TokenType}'.";
            return default;
        }

        if (!reader.Read() || !reader.TryGetDouble(out double latitude))
        {
            _ = $"Expected double, but got '{reader.TokenType}'.";
            return default;
        }

        if (!reader.Read() || !reader.TryGetDouble(out double longitude))
        {
            _ = $"Expected double, but got '{reader.TokenType}'.";
            return default;
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            _ = $"Expected end of array, but got '{reader.TokenType}'.";
            return default;
        }

        return new(longitude, latitude);
    }

    public override void Write(Utf8JsonWriter writer, Coordinates value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        writer.WriteNumberValue(value.Latitude);
        writer.WriteNumberValue(value.Longitude);

        writer.WriteEndArray();
    }
}
