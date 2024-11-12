using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.DistanceCalculationService;

public class CacheAccessException : DistanceCalculationServiceException
{
    public CacheAccessException()
    {
    }

    public CacheAccessException(string? message) : base(message)
    {
    }

    public CacheAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CacheAccessException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
