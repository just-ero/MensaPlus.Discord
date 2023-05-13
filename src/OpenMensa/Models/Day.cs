using System;
using System.Text.Json.Serialization;

namespace OpenMensa.Models;

public sealed record Day(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("closed")] bool Closed);
