namespace CarRent.Application.DTOs.Settings;

public class TaxSettingDto
{
    public decimal TaxRatePercent { get; set; }
}

public class UpdateTaxSettingRequest
{
    public decimal TaxRatePercent { get; set; }
}
