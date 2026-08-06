namespace CarRent.Application.DTOs.Drivers;

public class DriverSalaryPaymentDto
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal Deductions { get; set; }
    public decimal Bonus { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalaryPaymentRequest
{
    public Guid DriverId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal Deductions { get; set; }
    public decimal Bonus { get; set; }
    public string? Notes { get; set; }
}
