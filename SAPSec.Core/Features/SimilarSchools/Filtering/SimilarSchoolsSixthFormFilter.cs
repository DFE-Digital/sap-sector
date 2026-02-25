namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsSixthFormFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.OfficialSixthForm)
{
    public override string Name => "Sixth form";
}
