namespace CarRent.Domain.Entities;

public class PlanLimit
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string LimitKey { get; set; } = string.Empty;
    public int LimitValue { get; set; }
}
