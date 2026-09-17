namespace SAPSec.Core.Features.SchoolDetails;

public static class EstablishmentLinkTypeValues
{
    private const string SuccessorPrefix = "Successor";
    private const string PredecessorPrefix = "Predecessor";

    public static bool IsSuccessor(string? linkType) =>
        StartsWith(linkType, SuccessorPrefix);

    public static bool IsPredecessor(string? linkType) =>
        StartsWith(linkType, PredecessorPrefix);

    private static bool StartsWith(string? linkType, string prefix) =>
        !string.IsNullOrWhiteSpace(linkType)
        && linkType.Trim().StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
