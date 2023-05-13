using System.Text.Json.Serialization;

namespace OpenMensa.Models;

public sealed record Meal(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("prices")] Prices Prices,
    [property: JsonPropertyName("notes")] string[] Notes);
