using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

internal static class AttendanceMeasures
{
    public static class Absence
    {
        public static Measure ForSchool(SchoolData<AbsenceData> currentSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector, measureDataType) = ResolveFilters(filters);

            return Measure.ForSchoolAttendance(
                Constants.Measures.Absence.Key,
                Constants.Measures.Absence.Name,
                2023,
                measureDataType,
                availableFilters,
                currentSchool,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(
            SchoolData<AbsenceData> currentSchool,
            SchoolData<AbsenceData> similarSchool,
            CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector, measureDataType) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Constants.Measures.Absence.Key,
                Constants.Measures.Absence.Name,
                2023,
                measureDataType,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<AbsenceData> FieldSelector, MeasureDataType MeasureDataType) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var type = filters.ContainsKey(Constants.Measures.Absence.Filters.Type.Key)
                ? filters[Constants.Measures.Absence.Filters.Type.Key]
                : Constants.Measures.Absence.Filters.Type.Values.Overall;

            var measureDataType = type == Constants.Measures.Absence.Filters.Type.Values.Overall
                ? MeasureDataType.OverallAbsencePercentage
                : MeasureDataType.PersistentAbsencePercentage;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Constants.Measures.Absence.Filters.Type.Key,
                    Constants.Measures.Absence.Filters.Type.Name,
                    Constants.Measures.Absence.Filters.Type.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(type)))
                    .ToList())
            ];

            MeasureFieldSelector<AbsenceData> fieldSelector = type switch
            {
                _ when type.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.Type.Values.Persistent) => new(
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Current_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Previous_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Previous2_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Current_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Previous_Pct,
                    x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Previous2_Pct),
                _ => new(
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct,
                    x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Current_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Previous_Pct,
                    x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Previous2_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Current_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Previous_Pct,
                    x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Previous2_Pct)
            };

            return (availableFilters, fieldSelector, measureDataType);
        }
    }
}
