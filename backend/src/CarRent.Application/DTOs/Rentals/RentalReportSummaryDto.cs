namespace CarRent.Application.DTOs.Rentals;

public class RentalReportSummaryDto
{
    public int TotalRentals { get; set; }
    public int ActiveRentals { get; set; }
    public int ClosedRentals { get; set; }
    public double AverageTurnaroundHours { get; set; }
    public decimal TotalLateFees { get; set; }
    public decimal TotalDamageCharges { get; set; }
    public decimal TotalDepositsHeld { get; set; }
    public decimal TotalDepositsRefunded { get; set; }
    public decimal TotalDepositsForfeited { get; set; }
}
