using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using OpenMensa.Errors;
using OpenMensa.Extensions;
using OpenMensa.Models;
using OpenMensa.Serialization;

using Remora.Results;

namespace OpenMensa;

public sealed class OpenMensaApi
{
    private const string ApiUrl = "https://openmensa.org/api/v2/canteens";

    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _options;

    public OpenMensaApi(IHttpClientFactory clientFactory)
    {
        _client = clientFactory.CreateClient();

        _options = new()
        {
            Converters = { new CoordinatesConverter() }
        };
    }

    public async Task<Result<IEnumerable<Canteen>>> GetCanteensAsync(
        CancellationToken ct = default)
    {
        IEnumerable<Canteen> allCanteens = Enumerable.Empty<Canteen>();
        int page = 1;

        while (!ct.IsCancellationRequested)
        {
            string requestUri = UriHelper.BuildParams(
                ("page", page));

            using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                string msg = $"Unable to retrieve canteens.";
                return new WebRequestError(response.StatusCode, msg);
            }

            using Stream content = await response.Content.ReadAsStreamAsync(ct);
            IEnumerable<Canteen>? canteens = await JsonSerializer.DeserializeAsync<IEnumerable<Canteen>>(content, _options, ct);

            if (canteens is null)
            {
                string msg = "Received a JSON null literal.";
                return new InvalidDataError(msg);
            }

            if (!canteens.Any())
            {
                break;
            }

            allCanteens = allCanteens.Concat(canteens);

            page++;
        }

        return Result<IEnumerable<Canteen>>.FromSuccess(allCanteens);
    }

    public async Task<Result<IEnumerable<Canteen>>> GetCanteensByCityAsync(
        string city,
        CancellationToken ct = default)
    {
        Result<IEnumerable<Canteen>> rCanteens = await GetCanteensAsync(ct);
        if (!rCanteens.IsSuccess)
        {
            return rCanteens;
        }

        return Result<IEnumerable<Canteen>>
            .FromSuccess(rCanteens.Entity
                .Where(rCanteens => rCanteens.City == city));
    }

    public Task<Result<IEnumerable<Canteen>>> GetCanteensNearAsync(
        Coordinates coordinates,
        CancellationToken ct = default)
    {
        return GetCanteensNearAsync(coordinates, 10, ct);
    }

    public async Task<Result<IEnumerable<Canteen>>> GetCanteensNearAsync(
        Coordinates coordinates,
        double distanceInKm,
        CancellationToken ct = default)
    {
        IEnumerable<Canteen> allCanteens = Enumerable.Empty<Canteen>();
        int page = 1;

        while (true)
        {
            string requestUri = UriHelper.BuildParams(
                ("near[lat]", coordinates.Latitude),
                ("near[lng]", coordinates.Longitude),
                ("near[dist]", distanceInKm),
                ("page", page));

            using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

            if (!response.IsSuccessStatusCode)
            {
                string msg = $"Unable to retrieve canteens.";
                return new WebRequestError(response.StatusCode, msg);
            }

            using Stream content = await response.Content.ReadAsStreamAsync(ct);
            IEnumerable<Canteen>? canteens = await JsonSerializer.DeserializeAsync<IEnumerable<Canteen>>(content, _options, ct);

            if (canteens is null)
            {
                string msg = "Received a JSON null literal.";
                return new InvalidDataError(msg);
            }

            if (!canteens.Any())
            {
                break;
            }

            allCanteens = allCanteens.Concat(canteens);

            page++;
        }

        return Result<IEnumerable<Canteen>>.FromSuccess(allCanteens);
    }

    public async Task<Result<Canteen>> GetCanteenAsync(
        int id,
        CancellationToken ct = default)
    {
        string requestUri = UriHelper.Build(
            id);

        using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            string msg = $"Unable to retrieve canteen {id}.";
            return new WebRequestError(response.StatusCode, msg);
        }

        using Stream content = await response.Content.ReadAsStreamAsync(ct);
        Canteen? canteen = await JsonSerializer.DeserializeAsync<Canteen>(content, _options, ct);

        if (canteen is null)
        {
            string msg = "Received a JSON null literal.";
            return new InvalidDataError(msg);
        }

        return canteen;
    }

    public async Task<Result<Canteen>> GetCanteenAsync(
        string name,
        CancellationToken ct = default)
    {
        Result<IEnumerable<Canteen>> rCanteens = await GetCanteensAsync(ct);
        if (!rCanteens.IsSuccess)
        {
            return Result<Canteen>.FromError(rCanteens.Error);
        }

        Canteen? canteen = rCanteens.Entity.FirstOrDefault(c => c.Name == name);

        if (canteen is null)
        {
            return new NotFoundError($"No canteen matching '{name}' could be found.");
        }

        return canteen;
    }

    public async Task<Result<IEnumerable<Day>>> GetDaysAsync(
        int canteenId,
        CancellationToken ct = default)
    {
        string requestUri = UriHelper.Build(
            canteenId);

        using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            string msg = $"Unable to retrieve days for canteen {canteenId}.";
            return new WebRequestError(response.StatusCode, msg);
        }

        using Stream content = await response.Content.ReadAsStreamAsync(ct);
        IEnumerable<Day>? days = await JsonSerializer.DeserializeAsync<IEnumerable<Day>>(content, _options, ct);

        if (days is null)
        {
            string msg = "Received a JSON null literal.";
            return new InvalidDataError(msg);
        }

        return Result<IEnumerable<Day>>.FromSuccess(days);
    }

    public async Task<Result<Day>> GetDayAsync(
        int canteenId,
        DateTime date,
        CancellationToken ct = default)
    {
        string requestUri = UriHelper.Build(
            canteenId,
            "days",
            $"{date:yyyy-MM-dd}");

        using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            string msg = "Unable to retrieve the given day.";
            return new WebRequestError(response.StatusCode, msg);
        }

        using Stream content = await response.Content.ReadAsStreamAsync(ct);
        Day? day = await JsonSerializer.DeserializeAsync<Day>(content, _options, ct);

        if (day is null)
        {
            string msg = "Received a JSON null literal.";
            return new InvalidDataError(msg);
        }

        return day;
    }

    public async Task<Result<IEnumerable<Meal>>> GetMealsAsync(
        int canteenId,
        DateTime date,
        CancellationToken ct = default)
    {
        string requestUri = UriHelper.Build(
            canteenId,
            "days",
            $"{date:yyyy-MM-dd}",
            "meals");

        using HttpResponseMessage response = await _client.GetAsync(ApiUrl + requestUri, ct);

        if (!response.IsSuccessStatusCode)
        {
            string msg = "Unable to retrieve meals for the given day.";
            return new WebRequestError(response.StatusCode, msg);
        }

        using Stream content = await response.Content.ReadAsStreamAsync(ct);
        IEnumerable<Meal>? meals = await JsonSerializer.DeserializeAsync<IEnumerable<Meal>>(content, _options, ct);

        if (meals is null)
        {
            string msg = "Received a JSON null literal.";
            return new InvalidDataError(msg);
        }

        return Result<IEnumerable<Meal>>.FromSuccess(meals);
    }
}
