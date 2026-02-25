namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsAdmissionsPolicyFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.AdmissionsPolicy)
{
    public override string Name => "Admissions policy";
}
