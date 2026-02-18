namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsRegionFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.RegionId,
    s => s.RegionName)
{
    public override string Key => "reg";
    public override string Name => "Region";
}
