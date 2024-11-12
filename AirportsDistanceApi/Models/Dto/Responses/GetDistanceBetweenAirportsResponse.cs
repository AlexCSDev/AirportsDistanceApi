namespace AirportsDistanceApi.Models.Dto.Responses;

public record GetDistanceBetweenAirportsResponse : BaseResponse
{
    public double? Result { get; set; }
}
