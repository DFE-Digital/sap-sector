namespace SAPSec.Core.Features.SimilarSchools.Filtering;

public class SimilarSchoolsGenderOfEntryFilter(SimilarSchool currentSchool) : SimilarSchoolsIdAndNameFieldFilter(
    currentSchool,
    s => s.GenderId,
    s => s.GenderName)
{
    public override string Key => "g";
    public override string Name => "Gender of entry";
}
