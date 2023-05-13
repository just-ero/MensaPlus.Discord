using System.Text.Json.Serialization;

namespace OpenMensa.Models;

public sealed record Prices(
    [property: JsonPropertyName("students")] double? Students,
    [property: JsonPropertyName("employees")] double? Employees,
    [property: JsonPropertyName("pupils")] double? Pupils,
    [property: JsonPropertyName("others")] double? Others);
