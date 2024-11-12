using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.DistanceCalculationService;

public abstract class DistanceCalculationServiceException : Exception
{
    protected DistanceCalculationServiceException()
    {
    }

    protected DistanceCalculationServiceException(string? message) : base(message)
    {
    }

    protected DistanceCalculationServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected DistanceCalculationServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
