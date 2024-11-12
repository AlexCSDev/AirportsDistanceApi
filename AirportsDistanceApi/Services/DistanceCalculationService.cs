using AirportsDistanceApi.Exceptions.DistanceCalculationService;
using AirportsDistanceApi.Exceptions.SimpleRestApiClient;
using AirportsDistanceApi.Helpers;
using AirportsDistanceApi.Interfaces.Services;
using AirportsDistanceApi.Models.Dto.PlacesApi;
using GeoCoordinatePortable;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace AirportsDistanceApi.Services;

/// <inheritdoc/>
public class DistanceCalculationService : IDistanceCalculationService
{
    private readonly ISimpleRestApiClientService _simpleRestApiClient;
    private readonly IDatabase _redis;
    private readonly ILogger<DistanceCalculationService> _logger;

    public DistanceCalculationService(
        ISimpleRestApiClientService simpleRestApiClient,
        IConnectionMultiplexer muxer,
        ILogger<DistanceCalculationService> logger)
    {
        _simpleRestApiClient = simpleRestApiClient;
        _redis = muxer.GetDatabase();
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<double> GetDistanceBetweenTwoAirportsAsync(string airportACode, string airportBCode)
    {
        airportACode = airportACode.ToUpperInvariant();
        airportBCode = airportBCode.ToUpperInvariant();

        if (!IsValidIATACode(airportACode))
            throw new InvalidAirportCodeException($"\"{airportACode}\" is not a valid IATA code");

        if (!IsValidIATACode(airportBCode))
            throw new InvalidAirportCodeException($"\"{airportBCode}\" is not a valid IATA code");

        //Sort codes for caching purposes
        (airportACode, airportBCode) = SortAirportCodes(airportACode, airportBCode);

        double? distance;
        try
        {
            distance = await GetCacheAsync(airportACode, airportBCode);

            if (distance != null)
                return distance.Value;
        }
        catch(Exception ex) //note: this depends on requirements, if caching is not critical we can just silently swallow this and continue.
        {
            _logger.LogError(ex, $"Exception while retrieving cache: {ex.Message}, inner: {ex.InnerException?.Message ?? "no inner exception"}");

            throw new CacheAccessException($"Error while accessing cache: {ex.Message}", ex);
        }

        AirportDataDto airportAData = null!;
        AirportDataDto airportBData = null!;

        var aggregateTask = Task.WhenAll(GetDataAsync(airportACode), GetDataAsync(airportBCode));
        try
        {
            var taskResults = await aggregateTask;

            //Task.WhenAll guarantees the order will be preserved
            airportAData = taskResults[0];
            airportBData = taskResults[1];
        }
        catch(Exception)
        {
            if(aggregateTask.Exception != null)
                throw aggregateTask.Exception;

            throw; //failsafe
        }

        //Note: Haversine formula is used to calculate the distance.
        GeoCoordinate airportACoord = new GeoCoordinate(airportAData.Location.Latitude, airportAData.Location.Longitude);
        GeoCoordinate airportBCoord = new GeoCoordinate(airportBData.Location.Latitude, airportBData.Location.Longitude);

        distance = DataConversionHelper.ConvertMetersToMiles(airportACoord.GetDistanceTo(airportBCoord));

        try
        {
            await SetCacheAsync(airportACode, airportBCode, distance.Value);
        }
        catch (Exception ex) //note: this depends on requirements, if caching is not critical we can just silently swallow this and continue.
        {
            _logger.LogError(ex, $"Exception while setting cache: {ex.Message}, inner: {ex.InnerException?.Message ?? "no inner exception"}");

            throw new CacheAccessException($"Error while accessing cache: {ex.Message}", ex);
        }

        return distance.Value;
    }

    private async Task<AirportDataDto> GetDataAsync(string airportCode)
    {
        try
        {
            //simplification: in real world scenario the host would've been loaded from service configuration
            var data = await _simpleRestApiClient.GetAsync<AirportDataDto>($"https://places-dev.continent.ru/airports/{airportCode}");

            if (data == null)
                throw new DataRetrievalException($"Received empty response for airport {airportCode}");

            if (data.Location == null)
                throw new DataRetrievalException($"Received location data is invalid for airport {airportCode}");

            return data;
        }
        catch (SimpleRestApiClientException ex)
        {
            if (ex is UnsuccessfulRequestException unsuccEx && unsuccEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new InvalidAirportCodeException($"Airport with code \"{airportCode}\" does not exist");

            _logger.LogError(ex, $"Exception in GetDataAsync: {ex.Message}, inner: {ex.InnerException?.Message ?? "no inner exception"}");

            throw new DataRetrievalException($"Error while retrieving data for airport {airportCode}: {ex.Message}. Details: {ex.InnerException?.Message}", ex);
        }
    }
    private async Task<double?> GetCacheAsync(string airportACode, string airportBCode)
    {
        var cacheValue = await _redis.StringGetAsync($"DISTANCE_{airportACode}_{airportBCode}");
        if (cacheValue == RedisValue.Null || cacheValue == RedisValue.EmptyString)
            return null;

        if (double.TryParse(cacheValue, out var distance))
            return distance;

        return null;
    }

    private async Task SetCacheAsync(string airportACode, string airportBCode, double distance)
    {
        string key = $"DISTANCE_{airportACode}_{airportBCode}";
        var setTask = _redis.StringSetAsync(key, distance.ToString());
        var expireTask = _redis.KeyExpireAsync(key, TimeSpan.FromHours(1));

        await Task.WhenAll(setTask, expireTask);
    }

    private bool IsValidIATACode(string code)
    {
        return Regex.IsMatch(code, "^[A-Z]{3}$");
    }

    private (string, string) SortAirportCodes(string codeA, string codeB)
    {
        var orderedArr = new[] { codeA, codeB }.Order().ToArray();

        return (orderedArr[0], orderedArr[1]);

        //note: alternative implementation:
        /*var enumerator = new[] { codeA, codeB }.Order().GetEnumerator();
        enumerator.MoveNext();
        var str1 = enumerator.Current;
        enumerator.MoveNext();
        var str2 = enumerator.Current;
        return (str1, str2);*/
    }
}
