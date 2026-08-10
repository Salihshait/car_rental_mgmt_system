namespace CarRent.Application.DTOs.Ai;

public record PricingFactorDto(string Name, decimal Multiplier, string Explanation);

public record DynamicPricingResultDto(decimal BasePrice, decimal SuggestedPrice, List<PricingFactorDto> Factors);
