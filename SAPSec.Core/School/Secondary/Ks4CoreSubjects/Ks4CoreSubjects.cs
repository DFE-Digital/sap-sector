using SAPSec.Core.Filtering;
using SAPSec.Core.Measures;
using SAPSec.Data.Store;

namespace SAPSec.Core.School.Secondary.Ks4CoreSubjects;

internal static class Ks4CoreSubjects
{
    public static class EnglishLanguage
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> currentSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                currentSchool,
                similarSchools,
                filters,
                "english-language",
                "English language",
                new(
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "english-language",
                "English language",
                new(
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLang79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLang79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLang79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class EnglishLiterature
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> currentSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                currentSchool,
                similarSchools,
                filters,
                "english-literature",
                "English literature",
                new(
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "english-literature",
                "English literature",
                new(
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.EngLit79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.EngLit79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.EngLit79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Biology
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> schoolData, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "biology",
                "Biology",
                new(
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "biology",
                "Biology",
                new(
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Bio79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Bio79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Bio79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Chemistry
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> schoolData, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "chemistry",
                "Chemistry",
                new(
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "chemistry",
                "Chemistry",
                new(
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Chem79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Chem79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Chem79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Physics
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> schoolData, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "physics",
                "Physics",
                new(
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "physics",
                "Physics",
                new(
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Physics79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Physics79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Physics79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class Mathematics
    {
        public static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> schoolData, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "maths",
                "Mathematics",
                new(
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct));
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "maths",
                "Mathematics",
                new(
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.Maths79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.Maths79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.Maths79_Tot_Eng_Previous2_Pct));
        }
    }

    public static class CombinedScience
    {
        internal static Measure ForSecondarySchool(SecondarySchoolData<Ks4PerformanceData> schoolData, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchool(
                schoolData,
                similarSchools,
                filters,
                "combined-science",
                "Combined science (double award)",
                new(
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                hasCombinedGrades: true);
        }

        public static Measure ForSecondarySchoolComparison(SecondarySchoolData<Ks4PerformanceData> currentSchool, SecondarySchoolData<Ks4PerformanceData> similarSchool, IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools, IDictionary<string, string> filters)
        {
            return BuildSubjectMeasureForSchoolComparison(
                currentSchool,
                similarSchool,
                similarSchools,
                filters,
                "combined-science",
                "Combined science (double award)",
                new(
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci49_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci49_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci49_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci59_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci59_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci59_Tot_Eng_Previous2_Pct),
                new(
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Current_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous_Pct,
                    x => x?.EstablishmentPerformance?.CombSci79_Sum_Est_Previous2_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Current_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous_Pct,
                    x => x?.LocalAuthorityPerformance?.CombSci79_Tot_LA_Previous2_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Current_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous_Pct,
                    x => x?.EnglandPerformance?.CombSci79_Tot_Eng_Previous2_Pct),
                hasCombinedGrades: true);
        }
    }

    private static Measure BuildSubjectMeasureForSchool(
        SecondarySchoolData<Ks4PerformanceData> currentSchool,
        IEnumerable<SecondarySchoolData<Ks4PerformanceData>> similarSchools,
        IDictionary<string, string> filters,
        string key,
        string name,
        MeasureFieldSelector<Ks4PerformanceData> grade4Fields,
        MeasureFieldSelector<Ks4PerformanceData> grade5Fields,
        MeasureFieldSelector<Ks4PerformanceData> grade7Fields,
        bool hasCombinedGrades = false)
    {
        var filterKey = $"{key}:grade";
        var grade = filters.ContainsKey(filterKey) ? filters[filterKey] : "4";

        return Measure.ForSecondarySchool(
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
            currentSchool,
            similarSchools,
            grade switch
            {
                "5" => grade5Fields,
                "7" => grade7Fields,
                _ => grade4Fields
            });
    }

    private static Measure BuildSubjectMeasureForSchoolComparison<T>(
        SecondarySchoolData<T> currentSchool,
        SecondarySchoolData<T> similarSchool,
        IEnumerable<SecondarySchoolData<T>> similarSchools,
        IDictionary<string, string> filters,
        string key,
        string name,
        MeasureFieldSelector<T> grade4Fields,
        MeasureFieldSelector<T> grade5Fields,
        MeasureFieldSelector<T> grade7Fields,
        bool hasCombinedGrades = false)
    {
        var filterKey = $"{key}:grade";
        var grade = filters.ContainsKey(filterKey) ? filters[filterKey] : "4";

        return Measure.ForSecondarySchoolComparison(
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
            currentSchool,
            similarSchool,
            similarSchools,
            grade switch
            {
                "5" => grade5Fields,
                "7" => grade7Fields,
                _ => grade4Fields
            });
    }

}
