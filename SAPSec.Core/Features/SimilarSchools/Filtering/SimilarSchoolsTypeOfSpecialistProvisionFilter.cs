namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsTypeOfSpecialistProvisionFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.ResourcedProvision)
{
    public override string Name => "Type of specialist provision";
}
