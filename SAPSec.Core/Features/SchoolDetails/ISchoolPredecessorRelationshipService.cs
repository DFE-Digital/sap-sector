using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Determines whether a school should show the successor banner ("This school was previously ...")
/// and, if so, which predecessor schools to signpost to.
/// </summary>
public interface ISchoolPredecessorRelationshipService
{
    Task<SchoolPredecessorRelationship> EvaluateAsync(Establishment establishment);
}
