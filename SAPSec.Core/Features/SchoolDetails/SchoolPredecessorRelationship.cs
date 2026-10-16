namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// Result of evaluating whether a school (which is not itself closed) replaced one or more earlier
/// schools, and so should show the successor banner signposting back to them.
/// </summary>
public sealed record SchoolPredecessorRelationship(IReadOnlyList<SuccessorSchool> Predecessors)
{
    public static readonly SchoolPredecessorRelationship None = new([]);

    public bool ShowSuccessorBanner => Predecessors.Count > 0;
}
