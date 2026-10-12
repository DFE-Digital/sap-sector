//namespace SAPSec.Core.Features.SchoolDetails;

//public static class SchoolDetailsExtensions
//{
//    public static bool IsPrimarySchool(this SchoolDetails school)
//    {
//        ArgumentNullException.ThrowIfNull(school);

//        return school.PhaseOfEducation.HasValue
//            && PhaseOfEducationId.IsPrimaryOrAllThrough(school.PhaseOfEducation.Value);
//    }

//    public static bool IsAllThroughSchool(this SchoolDetails school)
//    {
//        ArgumentNullException.ThrowIfNull(school);

//        return school.PhaseOfEducation.HasValue
//            && PhaseOfEducationId.IsAllThrough(school.PhaseOfEducation.Value);
//    }

//    public static bool IsSecondarySchool(this SchoolDetails school)
//    {
//        ArgumentNullException.ThrowIfNull(school);

//        return school.PhaseOfEducation.HasValue
//            && PhaseOfEducationId.IsSecondary(school.PhaseOfEducation.Value);
//    }
//}
