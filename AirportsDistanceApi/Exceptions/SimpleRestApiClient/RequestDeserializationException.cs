using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.SimpleRestApiClient;

public class RequestDeserializationException : SimpleRestApiClientException
{
    public RequestDeserializationException()
    {
    }

    public RequestDeserializationException(string? message) : base(message)
    {
    }

    public RequestDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public RequestDeserializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
