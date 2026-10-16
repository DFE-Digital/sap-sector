namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Result of evaluating whether a closed school may still be displayed in the service, whether the
/// plain closed-school banner should be shown, and any successor schools to signpost to instead.
/// </summary>
public sealed record SchoolClosureEligibility(
    bool IsEligibleForDisplay,
    bool ShowClosedSchoolBanner,
    IReadOnlyList<SuccessorSchool> Successors)
{
    public static readonly SchoolClosureEligibility NotClosed = new(
        IsEligibleForDisplay: true,
        ShowClosedSchoolBanner: false,
        Successors: []);

    public bool ShowPredecessorBanner => Successors.Count > 0;
}
