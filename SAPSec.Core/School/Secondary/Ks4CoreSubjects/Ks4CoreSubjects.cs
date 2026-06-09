using SAPSec.Core.Filtering;
using SAPSec.Core.Measures;

namespace SAPSec.Core.School.Secondary.Ks4CoreSubjects;

internal static class Ks4CoreSubjects
{
    public static class EnglishLanguage
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "english-language",
                "English language",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "english-language",
                "English language",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class EnglishLiterature
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "english-literature",
                "English literature",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "english-literature",
                "English literature",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Biology
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "biology",
                "Biology",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "biology",
                "Biology",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Chemistry
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "chemistry",
                "Chemistry",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "chemistry",
                "Chemistry",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Physics
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "physics",
                "Physics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "physics",
                "Physics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Mathematics
    {
        public static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "maths",
                "Mathematics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "maths",
                "Mathematics",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class CombinedScience
    {
        internal static Measure ForSchool(SchoolData schoolData, IEnumerable<SchoolData> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "combined-science",
                "Combined science (double award)",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                hasCombinedGrades: true);
        }

        public static Measure ForSchoolComparison(SchoolData currentSchoolData, SchoolData similarSchoolData, IDictionary<string, SchoolData> similarSchoolsData, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                similarSchoolsData,
                filters,
                "combined-science",
                "Combined science (double award)",
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.PerformanceData?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.PerformanceData?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.PerformanceData?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                hasCombinedGrades: true);
        }
    }

    private static Measure BuildSubjectMeasureForSchool(
        SchoolData schoolData,
        IEnumerable<SchoolData> similarSchools,
        IDictionary<string, string> filters,
        string key,
        string name,
        MeasureFieldSelector grade4Fields,
        MeasureFieldSelector grade5Fields,
        MeasureFieldSelector grade7Fields,
        bool hasCombinedGrades = false)
    {
        var filterKey = $"{key}:grade";
        var grade = filters.ContainsKey(filterKey) ? filters[filterKey] : "4";

        return Measure.ForSchool(
            key,
            name,
            MeasureDataType.GradePercentage,
            [
                new MeasureAvailableFilter(
                    filterKey,
                    "Grade", [
                        new FilterOption("4", hasCombinedGrades ? "Grade 4-4 and above" : "Grade 4 and above", 0, grade == "4"),
                        new FilterOption("5", hasCombinedGrades ? "Grade 5-5 and above" : "Grade 5 and above", 0, grade == "5"),
                        new FilterOption("7", hasCombinedGrades ? "Grade 7-7 and above" : "Grade 7 and above", 0, grade == "7")
                    ]),
            ],
            schoolData,
            similarSchools,
            grade switch
            {
                "5" => grade5Fields,
                "7" => grade7Fields,
                _ => grade4Fields
            });
    }

    private static Measure BuildSubjectMeasureForSchoolComparison(
        SchoolData currentSchoolData,
        SchoolData similarSchoolData,
        IDictionary<string, SchoolData> similarSchoolsData,
        IDictionary<string, string> filters,
        string key,
        string name,
        MeasureFieldSelector grade4Fields,
        MeasureFieldSelector grade5Fields,
        MeasureFieldSelector grade7Fields,
        bool hasCombinedGrades = false)
    {
        var filterKey = $"{key}:grade";
        var grade = filters.ContainsKey(filterKey) ? filters[filterKey] : "4";

        return Measure.ForSchoolComparison(
            key,
            name,
            MeasureDataType.GradePercentage,
            [
                new MeasureAvailableFilter(
                    filterKey,
                    "Grade", [
                        new FilterOption("4", hasCombinedGrades ? "Grade 4-4 and above" : "Grade 4 and above", 0, grade == "4"),
                        new FilterOption("5", hasCombinedGrades ? "Grade 5-5 and above" : "Grade 5 and above", 0, grade == "5"),
                        new FilterOption("7", hasCombinedGrades ? "Grade 7-7 and above" : "Grade 7 and above", 0, grade == "7")
                    ]),
            ],
            currentSchoolData,
            similarSchoolData,
            similarSchoolsData.Values,
            grade switch
            {
                "5" => grade5Fields,
                "7" => grade7Fields,
                _ => grade4Fields
            });
    }

}
