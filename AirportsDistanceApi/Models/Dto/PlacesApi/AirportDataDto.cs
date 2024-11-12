using Newtonsoft.Json;

namespace AirportsDistanceApi.Models.Dto.PlacesApi;

public record AirportDataDto
{
    [JsonProperty("iata")]
    public string IATACode { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("city_iata")]
    public string CityIATA { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("country_iata")]
    public string CountryIATA { get; set; }

    [JsonProperty("location")]
    public LocationObject Location { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("hubs")]
    public int Hubs { get; set; }

    [JsonProperty("timezone_region_name")]
    public string TimezoneRegionName { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    public record LocationObject
    {
        [JsonProperty("lon")]
        public double Longitude { get; set; }

        [JsonProperty("lat")]
        public double Latitude { get; set; }
    }
}
