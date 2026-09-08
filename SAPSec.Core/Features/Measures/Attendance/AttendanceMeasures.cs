using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

internal static class AttendanceMeasures
{
    public static class Absence
    {
        public static Measure ForSchool(
            MeasurePhase phase,
            SchoolMeasureData<AbsenceData> currentSchool,
            CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector, measureDataType) = ResolveFilters(phase, filters);

            return Measure.ForSchoolAttendance(
                Constants.Measures.Absence.Key,
                Constants.Measures.Absence.Name,
                2024,
                measureDataType,
                availableFilters,
                currentSchool,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(
            MeasurePhase phase,
            SchoolMeasureData<AbsenceData> currentSchool,
            SchoolMeasureData<AbsenceData> similarSchool,
            CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector, measureDataType) = ResolveFilters(phase, filters);

            return Measure.ForSchoolComparison(
                Constants.Measures.Absence.Key,
                Constants.Measures.Absence.Name,
                2024,
                measureDataType,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<AbsenceData> FieldSelector, MeasureDataType MeasureDataType) ResolveFilters(
            MeasurePhase phase,
            CaseInsensitiveDictionary<string> filters)
        {
            var type = filters.ContainsKey(Constants.Measures.Absence.Filters.Type.Key)
                ? filters[Constants.Measures.Absence.Filters.Type.Key]
                : Constants.Measures.Absence.Filters.Type.Values.Overall;

            var characteristic = filters.ContainsKey(Constants.Measures.Absence.Filters.PupilCharacteristic.Key)
                ? filters[Constants.Measures.Absence.Filters.PupilCharacteristic.Key]
                : Constants.Measures.Absence.Filters.PupilCharacteristic.Values.AllPupils;

            var measureDataType = type == Constants.Measures.Absence.Filters.Type.Values.Overall
                ? MeasureDataType.OverallAbsencePercentage
                : MeasureDataType.PersistentAbsencePercentage;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Constants.Measures.Absence.Filters.Type.Key,
                    Constants.Measures.Absence.Filters.Type.Name,
                    Constants.Measures.Absence.Filters.Type.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(type)))
                    .ToList()),
                new MeasureAvailableFilter(
                    Constants.Measures.Absence.Filters.PupilCharacteristic.Key,
                    Constants.Measures.Absence.Filters.PupilCharacteristic.Name,
                    Constants.Measures.Absence.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(type, characteristic);
            var (laCurrent, laPrevious, laPrevious2, englandCurrent, englandPrevious, englandPrevious2) =
                ResolveLocalAuthorityAndEnglandAccessors(phase, type, characteristic);

            MeasureFieldSelector<AbsenceData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector, measureDataType);
        }

        private static (
            Func<AbsenceData?, string?> Current,
            Func<AbsenceData?, string?> Previous,
            Func<AbsenceData?, string?> Previous2) ResolveEstablishmentAccessors(string type, string characteristic)
        {
            return type switch
            {
                _ when type.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.Type.Values.Persistent) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Boy_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Boy_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Grl_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Grl_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Fsm) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Dis_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Dis_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.NonFsm) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_NDi_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_NDi_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_NDi_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EAL_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EAL_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EAL_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Efl) => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EFL_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EFL_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_EFL_Est_Previous2_Pct),
                    _ => (
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct),
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_Boy_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Boy_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Boy_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_Grl_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Grl_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Grl_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Fsm) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_Dis_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Dis_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Dis_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.NonFsm) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_NDi_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_NDi_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_NDi_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_EAL_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_EAL_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_EAL_Est_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Efl) => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_EFL_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_EFL_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_EFL_Est_Previous2_Pct),
                    _ => (
                        x => x?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct,
                        x => x?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct),
                },
            };
        }

        private static (
            Func<AbsenceData?, string?> LACurrent,
            Func<AbsenceData?, string?> LAPrevious,
            Func<AbsenceData?, string?> LAPrevious2,
            Func<AbsenceData?, string?> EnglandCurrent,
            Func<AbsenceData?, string?> EnglandPrevious,
            Func<AbsenceData?, string?> EnglandPrevious2) ResolveLocalAuthorityAndEnglandAccessors(MeasurePhase phase, string type, string characteristic)
        {
            return type switch
            {
                _ when type.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.Type.Values.Persistent) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Boys) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Boy_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Boy_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Boy_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Boy_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Boy_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Boy_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Boy_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Girls) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Grl_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Grl_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Grl_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Grl_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Grl_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Grl_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Grl_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Fsm) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Dis_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Dis_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Dis_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Dis_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Dis_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Dis_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Dis_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.NonFsm) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_NDi_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_NDi_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_NDi_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_NDi_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_NDi_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_NDi_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_NDi_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Eal) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EAL_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EAL_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EAL_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EAL_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EAL_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EAL_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EAL_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Efl) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_EFL_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EFL_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EFL_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EFL_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EFL_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_EFL_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_EFL_Secondary_Eng_Previous2_Pct),
                    _ => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Persistent_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Persistent_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Persistent_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Persistent_Secondary_Eng_Previous2_Pct),
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Boys) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Boy_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Boy_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Boy_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Boy_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Boy_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Boy_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Boy_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Girls) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Grl_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Grl_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Grl_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Grl_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Grl_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Grl_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Grl_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Fsm) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Dis_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Dis_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Dis_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Dis_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Dis_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Dis_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Dis_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.NonFsm) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_NDi_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_NDi_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_NDi_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_NDi_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_NDi_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_NDi_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_NDi_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Eal) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EAL_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EAL_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EAL_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EAL_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EAL_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EAL_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EAL_Secondary_Eng_Previous2_Pct),
                    _ when characteristic.EqualsCaseInsensitive(Constants.Measures.Absence.Filters.PupilCharacteristic.Values.Efl) => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_EFL_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EFL_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EFL_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EFL_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EFL_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_EFL_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_EFL_Secondary_Eng_Previous2_Pct),
                    _ => (
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Current_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Secondary_LA_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Previous_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Secondary_LA_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.LocalAuthorityAbsence?.Abs_Tot_Primary_LA_Previous2_Pct
                            : x => x?.LocalAuthorityAbsence?.Abs_Tot_Secondary_LA_Previous2_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Current_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Secondary_Eng_Current_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Previous_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Secondary_Eng_Previous_Pct,
                        phase is MeasurePhase.Primary
                            ? x => x?.EnglandAbsence?.Abs_Tot_Primary_Eng_Previous2_Pct
                            : x => x?.EnglandAbsence?.Abs_Tot_Secondary_Eng_Previous2_Pct),
                },
            };
        }
    }
}
