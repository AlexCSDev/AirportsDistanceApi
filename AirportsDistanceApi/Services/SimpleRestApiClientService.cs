using AirportsDistanceApi.Exceptions.SimpleRestApiClient;
using AirportsDistanceApi.Interfaces.Services;
using Newtonsoft.Json;

namespace AirportsDistanceApi.Services;

/// <inheritdoc/>
public class SimpleRestApiClientService : ISimpleRestApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SimpleRestApiClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc/>
    public async Task<TResult?> GetAsync<TResult>(string url)
        where TResult : class
    {
        var client = _httpClientFactory.CreateClient();

        HttpResponseMessage? responseMessage;
        string? responseContents;

        try
        {
            responseMessage = await client.GetAsync(url);
            responseContents = await responseMessage.Content.ReadAsStringAsync();
        }
        catch(Exception ex)
        {
			//note: in real life scenario we would have some code for handling retries depending on the type of failure.
            throw new UnsuccessfulRequestException("Error while receiving data. Details in inner exception.", ex);
        }

        if (!responseMessage.IsSuccessStatusCode)
            throw new UnsuccessfulRequestException(responseMessage.StatusCode, responseContents);

        try
        {
            return JsonConvert.DeserializeObject<TResult>(responseContents);
        }
        catch(Exception ex)
        {
            throw new RequestDeserializationException("Error while deserializing data. Details in inner exception.", ex);
        }
    }
}
