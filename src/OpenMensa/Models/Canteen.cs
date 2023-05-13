using System.Text.Json.Serialization;

namespace OpenMensa.Models;

public sealed record Canteen(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("coordinates")] Coordinates Coordinates);
