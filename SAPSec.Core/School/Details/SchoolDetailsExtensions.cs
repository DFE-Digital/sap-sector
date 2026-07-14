namespace SAPSec.Core.School.Details;

public static class SchoolDetailsExtensions
{
    public static bool IsPrimarySchool(this SchoolDetails school)
    {
        ArgumentNullException.ThrowIfNull(school);

        return school.PhaseOfEducation.HasValue
            && PhaseOfEducationValues.IsPrimaryOrAllThrough(school.PhaseOfEducation.Value);
    }

    public static bool IsSecondarySchool(this SchoolDetails school)
    {
        ArgumentNullException.ThrowIfNull(school);

        return school.PhaseOfEducation.HasValue
            && PhaseOfEducationValues.IsSecondary(school.PhaseOfEducation.Value);
    }
}
