namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsUrbanRuralFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.UrbanRuralId,
    s => s.UrbanRuralName)
{
    public override string Key => "ur";
    public override string Name => "Urban or rural";
}
