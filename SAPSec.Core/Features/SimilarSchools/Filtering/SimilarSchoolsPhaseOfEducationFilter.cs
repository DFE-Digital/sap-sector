namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsPhaseOfEducationFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.PhaseOfEducation)
{
    public override string Name => "Phase of education";
}
