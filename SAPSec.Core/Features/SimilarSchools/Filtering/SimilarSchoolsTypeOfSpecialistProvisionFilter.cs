namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsTypeOfSpecialistProvisionFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.ResourcedProvisionId,
    s => s.ResourcedProvisionName)
{
    public override string Key => "sp";
    public override string Name => "Type of specialist provision";
}
