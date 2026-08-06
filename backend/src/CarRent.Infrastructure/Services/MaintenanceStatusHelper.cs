namespace CarRent.Infrastructure.Services;

public static class MaintenanceStatusHelper
{
    private const int ExpiringSoonThresholdDays = 30;

    public static string ComputeExpiryStatus(string storedStatus, DateTime endDate)
    {
        if (storedStatus is "Cancelled" or "Voided")
        {
            return storedStatus;
        }

        var now = DateTime.UtcNow;
        if (endDate < now)
        {
            return "Expired";
        }

        return endDate <= now.AddDays(ExpiringSoonThresholdDays) ? "ExpiringSoon" : "Active";
    }
}
