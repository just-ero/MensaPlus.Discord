using System;
using System.Globalization;
using System.Text;

using OpenMensa.Models;

namespace MensaPlus.Discord.Extensions;

internal static class OpenMensaExtensions
{
    public static string ToDisplayString(this Prices prices, IFormatProvider? provider = default)
    {
        provider ??= CultureInfo.InvariantCulture;

        StringBuilder builder = new();

        if (prices.Students is double sPrice)
        {
            _ = builder
                .AppendPrice("Students", sPrice, provider)
                .AppendLine();
        }

        if (prices.Employees is double ePrice)
        {
            _ = builder
                .AppendPrice("Employees", ePrice, provider)
                .AppendLine();
        }

        if (prices.Pupils is double pPrice)
        {
            _ = builder
                .AppendPrice("Pupils", pPrice, provider)
                .AppendLine();
        }

        if (prices.Others is double oPrice)
        {
            _ = builder
                .AppendPrice("Others", oPrice, provider)
                .AppendLine();
        }

        return builder.ToString();
    }

    private static StringBuilder AppendPrice(this StringBuilder builder, string group, double price, IFormatProvider provider)
    {
        return builder
            .Append(group).Append(": ")
            .AppendFormat(provider, "{0:F2}", price)
            .Append(" €");
    }
}
