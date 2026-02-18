namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsSixthFormFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.OfficialSixthFormId,
    s => s.OfficialSixthFormName)
{
    public override string Key => "sf";
    public override string Name => "Sixth form";
}
