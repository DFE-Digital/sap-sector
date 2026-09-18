namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Result of evaluating whether a closed school may still be displayed in the service,
/// and whether the closed school banner should be shown.
/// </summary>
public sealed record SchoolClosureEligibility(bool IsEligibleForDisplay, bool ShowClosedSchoolBanner)
{
    public static readonly SchoolClosureEligibility NotClosed = new(IsEligibleForDisplay: true, ShowClosedSchoolBanner: false);
}
