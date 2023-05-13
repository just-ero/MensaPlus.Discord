using System;
using System.Drawing;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace MensaPlus.Discord.Extensions;

internal static class ResultExtensions
{
    public static IEmbed CreateErrorEmbed(this IResult result, string description)
    {
        if (result.Error?.Message is string error)
        {
            description =
                description +
                Environment.NewLine + Environment.NewLine +
                $"```{error}```";
        }

        return new Embed
        {
            Title = "MensaPlus Error",
            Description = description,
            Colour = Color.Red
        };
    }

    public static IEmbed CreateResponseErrorEmbed(this IResult result)
    {
        return result.CreateErrorEmbed("MensaPlus was unable to send the response properly.");
    }

    public static IEmbed CreateBuildErrorEmbed(this IResult result)
    {
        return result.CreateErrorEmbed("MensaPlus was unable to build the response embed.");
    }
}
