using AirportsDistanceApi.Exceptions.DistanceCalculationService;

namespace AirportsDistanceApi.Interfaces.Services;

/// <summary>
/// Distance calculator service
/// </summary>
public interface IDistanceCalculationService
{
    /// <summary>
    /// Get distance between two airports using Haversine formula.
    /// </summary>
    /// <param name="airportACode">IATA code of the first airport</param>
    /// <param name="airportBCode">IATA code of the second airport</param>
    /// <returns>Distance in miles</returns>
    /// <exception cref="DataRetrievalException">Thrown when errors encountered when accessing places api or parsing response from it</exception>
    /// <exception cref="CacheAccessException">Thrown when error encountered while accessing caching server</exception>
    /// <exception cref="InvalidAirportCodeException">Thrown when invalid IATA code supplied</exception>
    /// <exception cref="AggregateException">Contains one of the other exceptions</exception>
    public Task<double> GetDistanceBetweenTwoAirportsAsync(string airportACode, string airportBCode);
}
