using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.DistanceCalculationService;

public class InvalidAirportCodeException : DistanceCalculationServiceException
{
    public InvalidAirportCodeException()
    {
    }

    public InvalidAirportCodeException(string? message) : base(message)
    {
    }

    public InvalidAirportCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidAirportCodeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
