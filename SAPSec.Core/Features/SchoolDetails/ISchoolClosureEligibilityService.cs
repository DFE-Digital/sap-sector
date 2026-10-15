using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Determines whether a closed school may still be displayed in the service,
/// and whether the closed school banner should be shown for it.
/// </summary>
public interface ISchoolClosureEligibilityService
{
    Task<SchoolClosureEligibility> EvaluateAsync(Establishment establishment);
}
