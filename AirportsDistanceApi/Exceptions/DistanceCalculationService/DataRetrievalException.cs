using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.DistanceCalculationService;

public class DataRetrievalException : DistanceCalculationServiceException
{
    public DataRetrievalException()
    {
    }

    public DataRetrievalException(string? message) : base(message)
    {
    }

    public DataRetrievalException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public DataRetrievalException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
