namespace AirportsDistanceApi.Models.Dto.Responses;

public abstract record BaseResponse
{
    public bool IsSuccess { get; set; }
    public List<string>? ErrorMessages { get; set; }
}
