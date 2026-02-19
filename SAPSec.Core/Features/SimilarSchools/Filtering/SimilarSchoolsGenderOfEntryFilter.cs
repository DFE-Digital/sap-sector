namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsGenderOfEntryFilter(SimilarSchool currentSchool) : SimilarSchoolsReferenceDataFilter(
    currentSchool,
    s => s.Gender)
{
    public override string Name => "Gender of entry";
}
