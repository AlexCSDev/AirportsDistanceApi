using AirportsDistanceApi.Exceptions.DistanceCalculationService;
using AirportsDistanceApi.Interfaces.Services;
using AirportsDistanceApi.Models.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AirportsDistanceApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DistanceController : ControllerBase
{
    private readonly IDistanceCalculationService _distanceCalculationService;
    private readonly ILogger<DistanceController> _logger;

    public DistanceController(IDistanceCalculationService distanceCalculationService, ILogger<DistanceController> logger)
    {
        _distanceCalculationService = distanceCalculationService;
        _logger = logger;
    }

    /// <summary>
    /// Get the distance in miles between two airports. Note: Haversine formula is used to calculate the distance.
    /// </summary>
    /// <param name="airportA">IATA code of the first airport</param>
    /// <param name="airportB">IATA code of the second airport</param>
    [HttpGet("{airportA}/{airportB}")]
    public async Task<ActionResult<GetDistanceBetweenAirportsResponse>> GetDistanceBetweenAirportsAsync(string airportA, string airportB)
    {
        double result = 0;

        try
        {
            result = await _distanceCalculationService.GetDistanceBetweenTwoAirportsAsync(airportA, airportB);
        }
        catch(DistanceCalculationServiceException ex)
        {
            var resp = new GetDistanceBetweenAirportsResponse { IsSuccess = false, ErrorMessages = new List<string> { ex.Message } };

            if (ex is InvalidAirportCodeException)
                return BadRequest(resp);

            return StatusCode(500, resp);
        }
        catch(AggregateException ex)
        {
            var resp = new GetDistanceBetweenAirportsResponse { IsSuccess = false, ErrorMessages = new List<string>(ex.InnerExceptions.Count) };

            HttpStatusCode statusCode = HttpStatusCode.BadRequest;

            bool unexpectedExceptionOccured = false; 
            foreach (var innerException in ex.InnerExceptions)
            {
                //let's assume we don't want to show users exceptions we are not expecting to see
                if (!(innerException is DataRetrievalException) && !(innerException is InvalidAirportCodeException))
                {
                    _logger.LogError(innerException, $"Unexpected exception: {innerException}");

                    if (unexpectedExceptionOccured)
                        continue; //just so we don't duplicate error messages

                    resp.ErrorMessages.Add("One or more internal service errors occured.");

                    if(statusCode != HttpStatusCode.InternalServerError)
                        statusCode = HttpStatusCode.InternalServerError;

                    unexpectedExceptionOccured = true;
                    continue;
                }

                if (innerException is DataRetrievalException && statusCode != HttpStatusCode.InternalServerError)
                    statusCode = HttpStatusCode.InternalServerError;

                resp.ErrorMessages.Add(innerException.Message);
            }

            return StatusCode((int)statusCode, resp);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Unhandled exception in GetDistanceBetweenAirportsAsync: {ex}");

            return StatusCode(500, new GetDistanceBetweenAirportsResponse { IsSuccess = false, ErrorMessages = new List<string> { "Internal service error occured." } });
        }

        return Ok(new GetDistanceBetweenAirportsResponse { IsSuccess = true, Result = result });
    }
}
