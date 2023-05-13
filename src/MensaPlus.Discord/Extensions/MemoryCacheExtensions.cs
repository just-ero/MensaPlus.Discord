using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MensaPlus.Discord.Resources;

using Microsoft.Extensions.Caching.Memory;

using OpenMensa;
using OpenMensa.Errors;
using OpenMensa.Models;

using Remora.Results;

namespace MensaPlus.Discord.Extensions;

internal static class MemoryCacheExtensions
{
    public static async Task<Result<IEnumerable<Canteen>>> GetCanteensAsync(
        this IMemoryCache cache,
        OpenMensaApi openMensa,
        CancellationToken ct = default)
    {
        if (!cache.TryGetValue(MPR.CanteensKey, out IEnumerable<Canteen>? canteens))
        {
            Result<IEnumerable<Canteen>> rCanteens = await openMensa.GetCanteensAsync(ct);
            if (!rCanteens.IsSuccess)
            {
                return Result<IEnumerable<Canteen>>.FromError(rCanteens.Error);
            }

            canteens = rCanteens.Entity;
            _ = cache.Set(MPR.CanteensKey, canteens, TimeSpan.FromDays(1));
        }
        else if (canteens is null)
        {
            return new InvalidDataError("Canteens collection was `null` in memory cache.");
        }

        return Result<IEnumerable<Canteen>>.FromSuccess(canteens);
    }

    public static async Task<Result<Dictionary<string, Canteen>>> GetCanteensMapAsync(
        this IMemoryCache cache,
        OpenMensaApi openMensa,
        CancellationToken ct = default)
    {
        if (!cache.TryGetValue(MPR.CanteensMapKey, out Dictionary<string, Canteen>? map))
        {
            Result<IEnumerable<Canteen>> rCanteens = await cache.GetCanteensAsync(openMensa, ct);
            if (!rCanteens.IsSuccess)
            {
                return Result<Dictionary<string, Canteen>>.FromError(rCanteens.Error);
            }

            map = new();

            foreach (Canteen canteen in rCanteens.Entity)
            {
                map[canteen.Name] = canteen;
            }

            _ = cache.Set(MPR.CanteensMapKey, map, TimeSpan.FromDays(1));
        }
        else if (map is null)
        {
            return new InvalidDataError("Canteen.Name:Canteen map was `null` in memory cache.");
        }

        return map;
    }

    public static async Task<Result<string[]>> GetCanteenNamesAsync(
        this IMemoryCache cache,
        OpenMensaApi openMensa,
        CancellationToken ct = default)
    {
        if (!cache.TryGetValue(MPR.CanteenNamesKey, out string[]? names))
        {
            Result<IEnumerable<Canteen>> rCanteens = await cache.GetCanteensAsync(openMensa, ct);
            if (!rCanteens.IsSuccess)
            {
                return Result<string[]>.FromError(rCanteens.Error);
            }

            names = rCanteens.Entity.Select(c => c.Name).ToArray();
            _ = cache.Set(MPR.CanteenNamesKey, names, TimeSpan.FromDays(1));
        }
        else if (names is null)
        {
            return new InvalidDataError("Canteen names collection was `null` in memory cache.");
        }

        return names;
    }
}
