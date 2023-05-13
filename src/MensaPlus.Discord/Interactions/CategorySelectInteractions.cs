using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using MensaPlus.Discord.Extensions;

using Microsoft.Extensions.Caching.Memory;

using OpenMensa.Errors;
using OpenMensa.Models;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace MensaPlus.Discord.Interactions;

public sealed class CategorySelectInteractions : InteractionGroup
{
    private readonly IInteractionCommandContext _context;
    private readonly IDiscordRestInteractionAPI _api;
    private readonly IMemoryCache _cache;

    private readonly CultureInfo _culture;

    public CategorySelectInteractions(
        IInteractionCommandContext context,
        IDiscordRestInteractionAPI api,
        IMemoryCache cache)
    {
        _context = context;
        _api = api;
        _cache = cache;

        _culture = CultureInfo.GetCultureInfo(_context.Interaction.GuildLocale.Value);
    }

    [SelectMenu("categories-dropdown")]
    public async Task<Result> DisplayMealsInCategory(IReadOnlyList<string> values)
    {
        if (!_context.Interaction.Message.TryGet(out IMessage? message))
        {
            return new InvalidOperationError("Interaction without a message?");
        }

        if (values.Count != 1)
        {
            return new InvalidOperationError("Only one category may be selected at a time.");
        }

        Result resetResult = await ResetDropdown(message);
        if (!resetResult.IsSuccess)
        {
            return resetResult;
        }

        string key = values[0];
        if (!_cache.TryGetValue(key, out Meal[]? meals))
        {
            return new InvalidOperationError($"Category key '{key}' was not present in the memory cache.");
        }

        if (meals is null)
        {
            return new InvalidDataError($"Meals for category key '{key}' were `null`.");
        }

        Embed[] embeds = new Embed[meals.Length];
        for (int i = 0; i < meals.Length; i++)
        {
            Meal meal = meals[i];

            embeds[i] = new Embed(
                Title: meal.Name,
                Description: meal.Prices.ToDisplayString(_culture),
                Footer: new EmbedFooter(string.Join(", ", meal.Notes)));
        }

        return (Result)await _api.CreateFollowupMessageAsync(
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            flags: MessageFlags.Ephemeral,
            embeds: embeds);
    }

    private async Task<Result> ResetDropdown(IMessage message)
    {
        if (message.Components.Value.FirstOrDefault() is not IActionRowComponent actionRow)
        {
            string msg = "Could not find an ActionRowComponent in the original message.";
            return new InvalidOperationError(msg);
        }

        if (actionRow.Components.FirstOrDefault() is not IStringSelectComponent dropdown)
        {
            string msg = "Could not find a StringSelectComponent in the original message.";
            return new InvalidOperationError(msg);
        }

        return (Result)await _api.EditOriginalInteractionResponseAsync(
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            content: message.Content,
            embeds: new(message.Embeds),
            components: new[]
            {
                new ActionRowComponent(new[]
                {
                    new StringSelectComponent(
                        dropdown.CustomID,
                        dropdown.Options,
                        dropdown.Placeholder)
                })
            });
    }
}
