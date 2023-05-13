using System.Text.Json;

namespace MensaPlus.Discord.Extensions;

internal static class StringExtensions
{
    private static readonly JsonSerializerOptions s_options = new()
    {
        WriteIndented = true
    };

    public static string ToJsonString<T>(this T value)
    {
        return JsonSerializer.Serialize(value, s_options);
    }
}
