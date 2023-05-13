using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FuzzySharp;

using MensaPlus.Discord.Extensions;
using MensaPlus.Discord.Providers;
using MensaPlus.Discord.Resources;

using Microsoft.Extensions.Caching.Memory;

using OpenMensa;
using OpenMensa.Models;

using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Extensions.Formatting;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace MensaPlus.Discord.Commands;

public class MenuCommands : CommandGroup
{
    private readonly IInteractionCommandContext _context;
    private readonly IDiscordRestInteractionAPI _api;
    private readonly IMemoryCache _cache;
    private readonly OpenMensaApi _openMensa;

    public MenuCommands(
        IInteractionCommandContext context,
        IDiscordRestInteractionAPI api,
        IMemoryCache cache,
        OpenMensaApi openMensa)
    {
        _context = context;
        _api = api;
        _cache = cache;
        _openMensa = openMensa;
    }

    [Command("menu")]
    [Description("Gets the menu items for the specified canteen.")]
    public async Task<IResult> GetMenu(
        [Description("The canteen for which to retrieve the menu.")]
        [AutocompleteProvider(CanteenAutocompleteProvider.Identity)]
        string canteen,
        [Description("The date for which to retrieve the menu. If omitted, today's date is used.")]
        DateTime? date = default)
    {
        date ??= DateTime.Now;

        Result<Canteen> rCanteen = await GetCanteenFromName(canteen);
        if (!rCanteen.IsSuccess)
        {
            return rCanteen;
        }

        Canteen c = rCanteen.Entity.Convert();

        Result<(int MealCount, IEnumerable<ISelectOption> Options)> rOptions = await GenerateCategoryOptions(c.Id, date.Value);
        if (!rOptions.IsSuccess)
        {
            return rOptions;
        }

        int meals = rOptions.Entity.MealCount;
        IEnumerable<ISelectOption> options = rOptions.Entity.Options;

        if (meals == 0)
        {
            return (Result)await _api.EditOriginalInteractionResponseAsync(_context.Interaction.ApplicationID, _context.Interaction.Token,
                content: $"***{c.Name}** ({c.City})* offers __no__ meals on {Markdown.Timestamp(date.Value, TimestampStyle.LongDate)}.",
                embeds: Array.Empty<IEmbed>(),
                ct: CancellationToken);
        }

        string content = meals == 1
            ? $"***{c.Name}** ({c.City})* offers 1 meal on {Markdown.Timestamp(date.Value, TimestampStyle.LongDate)}:"
            : $"***{c.Name}** ({c.City})* offers {meals} meals on {Markdown.Timestamp(date.Value, TimestampStyle.LongDate)}:";

        return (Result)await _api.EditOriginalInteractionResponseAsync(_context.Interaction.ApplicationID, _context.Interaction.Token,
            content: content,
            embeds: Array.Empty<IEmbed>(),
            components: new[]
            {
                new ActionRowComponent(new[]
                {
                    new StringSelectComponent(
                        CustomID: CustomIDHelpers.CreateSelectMenuID("categories-dropdown"),
                        Placeholder: "Select a menu category to display its meals",
                        Options: options.ToArray())
                })
            },
            ct: CancellationToken);
    }

    private async Task<Result<Canteen>> GetCanteenFromName(string name)
    {
        _ = await EditOriginalInteractionResponseWithSimpleEmbedAsync(
            "Fetching canteen information...",
            MPR.DiscordInfoColor,
            CancellationToken);

        Result<Dictionary<string, Canteen>> rMap = await _cache.GetCanteensMapAsync(_openMensa, CancellationToken);
        if (!rMap.IsSuccess)
        {
            return Result<Canteen>.FromError(rMap.Error);
        }

        if (!rMap.Entity.TryGetValue(name, out Canteen? canteen))
        {
            Result<IEnumerable<Canteen>> rCanteens = await _cache.GetCanteensAsync(_openMensa, CancellationToken);
            if (!rCanteens.IsSuccess)
            {
                return Result<Canteen>.FromError(rCanteens.Error);
            }

            canteen = rCanteens.Entity
                .OrderByDescending(c => Fuzz.WeightedRatio(c.Name, name))
                .First();
        }

        return canteen;
    }

    private async Task<Result<(int MealCount, IEnumerable<ISelectOption> Options)>> GenerateCategoryOptions(
        int canteenId,
        DateTime dt)
    {
        _ = await EditOriginalInteractionResponseWithSimpleEmbedAsync(
            $"Fetching meals for canteen `{canteenId}`...",
            MPR.DiscordInfoColor,
            CancellationToken);

        Result<IEnumerable<Meal>> rMeals = await _openMensa.GetMealsAsync(canteenId, dt, CancellationToken);
        if (!rMeals.IsSuccess)
        {
            return Result<(int, IEnumerable<ISelectOption>)>.FromError(rMeals.Error);
        }

        _ = await EditOriginalInteractionResponseWithSimpleEmbedAsync(
            "Reading meal information...",
            MPR.DiscordInfoColor,
            CancellationToken);

        return Result<(int, IEnumerable<ISelectOption>)>
            .FromSuccess((
                rMeals.Entity.Count(),
                rMeals.Entity
                .Select(m => m.Convert(canteenId))
                .GroupBy(m => m.Category)
                .Select(g => (Name: g.Key, Count: g.Count(), Meals: g.Select(m => m).ToArray()))
                .OrderBy(c => c.Name)
                .Select((c, i) =>
                {
                    string id = $"{canteenId}:{dt:yyyy-MM-dd}:{i}";
                    _ = _cache.Set(id, c.Meals, dt.AddDays(3));

                    string desc = c.Count == 1
                        ? $"1 meal in this category."
                        : $"{c.Count} meals in this category.";

                    return new SelectOption(
                        Value: id,
                        Label: c.Name,
                        Description: desc);
                })));
    }

    private async Task<Result> EditOriginalInteractionResponseWithSimpleEmbedAsync(
        string content,
        Color color,
        CancellationToken ct = default)
    {
        return (Result)await _api.EditOriginalInteractionResponseAsync(_context.Interaction.ApplicationID, _context.Interaction.Token,
            embeds: new[]
            {
                new Embed(
                    Description: content,
                    Colour: color)
            },
            ct: ct);
    }
}
