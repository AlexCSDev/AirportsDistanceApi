namespace AirportsDistanceApi.Interfaces.Services;

/// <summary>
/// Simple REST Api client
/// </summary>
public interface ISimpleRestApiClientService
{
    /// <summary>
    /// Perform GET request which expects JSON response. Then deserialize that response into supplied type.
    /// </summary>
    /// <typeparam name="TResult">Type to deserialize into</typeparam>
    /// <param name="url">Url to access</param>
    /// <returns></returns>
    public Task<TResult?> GetAsync<TResult>(string url)
        where TResult : class;
}
