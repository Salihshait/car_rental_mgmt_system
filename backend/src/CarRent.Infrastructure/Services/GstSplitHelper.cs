namespace CarRent.Infrastructure.Services;

public static class GstSplitHelper
{
    public record GstSplitResult(decimal Cgst, decimal Sgst, decimal Igst);

    /// <summary>
    /// Splits an already-computed tax total into CGST+SGST (intra-state) or IGST (inter-state).
    /// A customer with no billing state on file defaults to intra-state, since most rentals are local.
    /// </summary>
    public static GstSplitResult Split(decimal taxTotal, string? branchState, string? customerBillingState)
    {
        if (taxTotal <= 0)
        {
            return new GstSplitResult(0, 0, 0);
        }

        var isIntraState = string.IsNullOrWhiteSpace(customerBillingState)
            || string.Equals(branchState?.Trim(), customerBillingState.Trim(), StringComparison.OrdinalIgnoreCase);

        if (!isIntraState)
        {
            return new GstSplitResult(0, 0, taxTotal);
        }

        var cgst = Math.Round(taxTotal / 2, 2);
        var sgst = taxTotal - cgst;
        return new GstSplitResult(cgst, sgst, 0);
    }
}
