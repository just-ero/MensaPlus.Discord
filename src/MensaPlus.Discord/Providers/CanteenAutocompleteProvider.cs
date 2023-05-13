using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FuzzySharp;

using MensaPlus.Discord.Extensions;

using Microsoft.Extensions.Caching.Memory;

using OpenMensa;

using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Autocomplete;

namespace MensaPlus.Discord.Providers;

public sealed class CanteenAutocompleteProvider : IAutocompleteProvider
{
    public const string Identity = "MensaPlus::Autocomplete::Canteen";

    private readonly IMemoryCache _cache;
    private readonly OpenMensaApi _openMensa;

    public CanteenAutocompleteProvider(IMemoryCache cache, OpenMensaApi openMensa)
    {
        _cache = cache;
        _openMensa = openMensa;
    }

    string IAutocompleteProvider.Identity
    {
        get => Identity;
    }

    public async ValueTask<IReadOnlyList<IApplicationCommandOptionChoice>> GetSuggestionsAsync(
        IReadOnlyList<IApplicationCommandInteractionDataOption> options,
        string userInput,
        CancellationToken ct = default)
    {
        Remora.Results.Result<string[]> rNames = await _cache.GetCanteenNamesAsync(_openMensa, ct);
        if (!rNames.IsSuccess)
        {
            return Array.Empty<IApplicationCommandOptionChoice>();
        }

        return rNames.Entity
            .OrderByDescending(name => Fuzz.WeightedRatio(name, userInput))
            .Take(20)
            .Select(name => new ApplicationCommandOptionChoice(name, name))
            .ToList();
    }
}
