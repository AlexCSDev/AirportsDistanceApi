using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.SimpleRestApiClient;

public abstract class SimpleRestApiClientException : Exception
{
    protected SimpleRestApiClientException()
    {
    }

    protected SimpleRestApiClientException(string? message) : base(message)
    {
    }

    protected SimpleRestApiClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected SimpleRestApiClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
