using SAPSec.Core.Constants;
using SAPSec.Core.Model;

namespace SAPSec.Core.Extensions;

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
