namespace AirportsDistanceApi.Helpers;

public static class DataConversionHelper
{
    public static double ConvertMetersToMiles(double meters)
    {
        return (meters / 1609.344);
    }
}
