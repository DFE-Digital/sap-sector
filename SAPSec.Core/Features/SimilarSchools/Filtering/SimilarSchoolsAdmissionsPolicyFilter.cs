namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsAdmissionsPolicyFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.AdmissionsPolicyId,
    s => s.AdmissionsPolicyName)
{
    public override string Key => "ap";
    public override string Name => "Admissions policy";
}
