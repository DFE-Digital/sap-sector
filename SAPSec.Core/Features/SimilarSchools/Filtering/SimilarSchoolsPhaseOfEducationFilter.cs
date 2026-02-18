namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsPhaseOfEducationFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.PhaseOfEducationId,
    s => s.PhaseOfEducationName)
{
    public override string Key => "poe";
    public override string Name => "Phase of education";
}
