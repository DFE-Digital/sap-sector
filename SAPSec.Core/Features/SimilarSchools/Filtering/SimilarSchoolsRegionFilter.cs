namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsRegionFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.Region)
{
    public override string Name => "Region";
}
