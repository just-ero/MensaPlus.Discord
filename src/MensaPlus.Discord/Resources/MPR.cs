using System.Drawing;

namespace MensaPlus.Discord.Resources;

internal static class MPR
{
    public const string CanteensKey = "MensaPlus::Canteens";
    public const string CanteensMapKey = "MensaPlus::Canteens::Map";
    public const string CanteenNamesKey = "MensaPlus::Canteens::Names";

    public static Color MensaPlusColor { get; } = Color.FromArgb(0x4C, 0xB0, 0x51);
    public static Color DiscordInfoColor { get; } = Color.FromArgb(0x58, 0x65, 0xF2);
    public static Color DiscordWarnColor { get; } = Color.FromArgb(0xFB, 0xB9, 0x48);
}
