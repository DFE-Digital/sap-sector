namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsUrbanRuralFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.UrbanRural)
{
    public override string Name => "Urban or rural";
}
