namespace CarRent.Infrastructure.Services;

public static class FleetStatusHelper
{
    public static string Compute(string vehicleStatus, bool isRented, bool isInMaintenance, bool isInTransit)
    {
        if (isRented)
        {
            return "Rented";
        }

        if (isInMaintenance)
        {
            return "InMaintenance";
        }

        if (isInTransit)
        {
            return "InTransit";
        }

        return vehicleStatus;
    }

    public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
