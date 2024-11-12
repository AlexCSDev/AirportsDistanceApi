using System.Net;
using System.Runtime.Serialization;

namespace AirportsDistanceApi.Exceptions.SimpleRestApiClient;

public class UnsuccessfulRequestException : SimpleRestApiClientException
{
    public HttpStatusCode? StatusCode { get; init; }
    public string? Contents { get; init; }

    public UnsuccessfulRequestException(HttpStatusCode statusCode, string contents) : base($"Unsuccessful request, error code: {statusCode}.")
    {
        StatusCode = statusCode;
        Contents = contents;
    }

    public UnsuccessfulRequestException(string? message) : base(message)
    {
    }

    public UnsuccessfulRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public UnsuccessfulRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
