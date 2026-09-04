using SAPSec.Core.Collections;
using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Data.Repositories;
using static SAPSec.Core.Constants.Measures.Primary;

namespace SAPSec.Core.Features.Measures.Primary;

internal static class Ks2PerformanceMeasures
{
    public static class MeetingExpectedStandardRwm
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2ExpectedRwm.Key,
                Ks2ExpectedRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2ExpectedRwm.Key,
                Ks2ExpectedRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var subject = filters.ContainsKey(Ks2ExpectedRwm.Filters.Subject.Key)
                ? filters[Ks2ExpectedRwm.Filters.Subject.Key]
                : Ks2ExpectedRwm.Filters.Subject.Values.ReadingWritingMaths;

            var characteristic = filters.ContainsKey(Ks2ExpectedRwm.Filters.PupilCharacteristic.Key)
                ? filters[Ks2ExpectedRwm.Filters.PupilCharacteristic.Key]
                : Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2ExpectedRwm.Filters.Subject.Key,
                    Ks2ExpectedRwm.Filters.Subject.Name,
                    Ks2ExpectedRwm.Filters.Subject.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(subject)))
                    .ToList()),
                new MeasureAvailableFilter(
                    Ks2ExpectedRwm.Filters.PupilCharacteristic.Key,
                    Ks2ExpectedRwm.Filters.PupilCharacteristic.Name,
                    Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(subject, characteristic);
            var (laCurrent, laPrevious, laPrevious2, englandCurrent, englandPrevious, englandPrevious2) =
                ResolveLocalAuthorityAndEnglandAccessors(subject, characteristic);

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector);
        }

        private static (
            Func<Ks2PerformanceData?, string?> Current,
            Func<Ks2PerformanceData?, string?> Previous,
            Func<Ks2PerformanceData?, string?> Previous2) ResolveEstablishmentAccessors(string subject, string characteristic)
        {
            return subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Reading) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Reading_Tot_Cohort_Est_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Writing) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Writing_Tot_Cohort_Est_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Maths) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Maths_Tot_Cohort_Est_Previous2_Num)
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmExpected_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmExpected_Tot_Cohort_Est_Previous2_Num)
                }
            };
        }

        private static (
            Func<Ks2PerformanceData?, string?> LACurrent,
            Func<Ks2PerformanceData?, string?> LAPrevious,
            Func<Ks2PerformanceData?, string?> LAPrevious2,
            Func<Ks2PerformanceData?, string?> EnglandCurrent,
            Func<Ks2PerformanceData?, string?> EnglandPrevious,
            Func<Ks2PerformanceData?, string?> EnglandPrevious2) ResolveLocalAuthorityAndEnglandAccessors(string subject, string characteristic)
        {
            // "Non-mobile pupils" is not published at LA or England level in the source data (DfE
            // performance tables only break this characteristic out at establishment level) - this
            // is expected, not a gap. Always show "no data" for LA/England when it's selected.
            if (characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonMobile))
            {
                return (x => null, x => null, x => null, x => null, x => null, x => null);
            }

            return subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Reading) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Writing) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.Subject.Values.Maths) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num)
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmExpected_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmExpected_Tot_Cohort_Eng_Previous2_Num)
                }
            };
        }
    }

    public static class AchievedHigherStandardRwm
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2HigherRwm.Key,
                Ks2HigherRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2HigherRwm.Key,
                Ks2HigherRwm.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var subject = filters.ContainsKey(Ks2HigherRwm.Filters.Subject.Key)
                ? filters[Ks2HigherRwm.Filters.Subject.Key]
                : Ks2HigherRwm.Filters.Subject.Values.ReadingWritingMaths;

            var characteristic = filters.ContainsKey(Ks2HigherRwm.Filters.PupilCharacteristic.Key)
                ? filters[Ks2HigherRwm.Filters.PupilCharacteristic.Key]
                : Ks2HigherRwm.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2HigherRwm.Filters.Subject.Key,
                    Ks2HigherRwm.Filters.Subject.Name,
                    Ks2HigherRwm.Filters.Subject.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(subject)))
                    .ToList()),
                new MeasureAvailableFilter(
                    Ks2HigherRwm.Filters.PupilCharacteristic.Key,
                    Ks2HigherRwm.Filters.PupilCharacteristic.Name,
                    Ks2HigherRwm.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            var (schoolCurrent, schoolPrevious, schoolPrevious2) = ResolveEstablishmentAccessors(subject, characteristic);
            var (laCurrent, laPrevious, laPrevious2, englandCurrent, englandPrevious, englandPrevious2) =
                ResolveLocalAuthorityAndEnglandAccessors(subject, characteristic);

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = new(
                schoolCurrent, schoolPrevious, schoolPrevious2,
                laCurrent, laPrevious, laPrevious2,
                englandCurrent, englandPrevious, englandPrevious2);

            return (availableFilters, fieldSelector);
        }

        private static (
            Func<Ks2PerformanceData?, string?> Current,
            Func<Ks2PerformanceData?, string?> Previous,
            Func<Ks2PerformanceData?, string?> Previous2) ResolveEstablishmentAccessors(string subject, string characteristic)
        {
            return subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Reading) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Reading_Tot_Cohort_Est_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Writing) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Writing_Tot_Cohort_Est_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Maths) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Maths_Tot_Cohort_Est_Previous2_Num)
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Boy_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Boy_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Boy_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Grl_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Grl_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Grl_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Dis_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Dis_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Dis_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_NDi_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_NDi_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_NDi_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_EAL_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_EAL_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_EAL_Cohort_Est_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonMobile) => (
                        x => x?.EstablishmentPerformance?.RwmHigher_NMo_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_NMo_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_NMo_Cohort_Est_Previous2_Num),
                    _ => (
                        x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Current_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Previous_Num,
                        x => x?.EstablishmentPerformance?.RwmHigher_Tot_Cohort_Est_Previous2_Num)
                }
            };
        }

        private static (
            Func<Ks2PerformanceData?, string?> LACurrent,
            Func<Ks2PerformanceData?, string?> LAPrevious,
            Func<Ks2PerformanceData?, string?> LAPrevious2,
            Func<Ks2PerformanceData?, string?> EnglandCurrent,
            Func<Ks2PerformanceData?, string?> EnglandPrevious,
            Func<Ks2PerformanceData?, string?> EnglandPrevious2) ResolveLocalAuthorityAndEnglandAccessors(string subject, string characteristic)
        {
            // "Non-mobile pupils" is not published at LA or England level in the source data (DfE
            // performance tables only break this characteristic out at establishment level) - this
            // is expected, not a gap. Always show "no data" for LA/England when it's selected.
            if (characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonMobile))
            {
                return (x => null, x => null, x => null, x => null, x => null, x => null);
            }

            return subject switch
            {
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Reading) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Reading_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Reading_Tot_Cohort_Eng_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Writing) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Writing_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Writing_Tot_Cohort_Eng_Previous2_Num)
                },
                _ when subject.EqualsCaseInsensitive(Ks2HigherRwm.Filters.Subject.Values.Maths) => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Maths_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Maths_Tot_Cohort_Eng_Previous2_Num)
                },
                _ => characteristic switch
                {
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Boys) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Boy_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Boy_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Boy_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Boy_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Boy_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Boy_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Girls) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Grl_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Grl_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Grl_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Grl_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Grl_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Grl_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Disadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Dis_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Dis_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Dis_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Dis_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Dis_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Dis_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.NonDisadvantaged) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_NDi_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_NDi_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_NDi_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_NDi_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_NDi_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_NDi_Cohort_Eng_Previous2_Num),
                    _ when characteristic.EqualsCaseInsensitive(Ks2HigherRwm.Filters.PupilCharacteristic.Values.Eal) => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_EAL_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_EAL_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_EAL_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_EAL_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_EAL_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_EAL_Cohort_Eng_Previous2_Num),
                    _ => (
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Current_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Previous_Num,
                        x => x?.LocalAuthorityPerformance?.RwmHigher_Tot_Cohort_LA_Previous2_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Current_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Previous_Num,
                        x => x?.EnglandPerformance?.RwmHigher_Tot_Cohort_Eng_Previous2_Num)
                }
            };
        }
    }

    public static class AverageScaledScoreReading
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2ReadingScore.Key,
                Ks2ReadingScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2ReadingScore.Key,
                Ks2ReadingScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var characteristic = filters.ContainsKey(Ks2ReadingScore.Filters.PupilCharacteristic.Key)
                ? filters[Ks2ReadingScore.Filters.PupilCharacteristic.Key]
                : Ks2ReadingScore.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2ReadingScore.Filters.PupilCharacteristic.Key,
                    Ks2ReadingScore.Filters.PupilCharacteristic.Name,
                    Ks2ReadingScore.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.Boys) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Boy_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Boy_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Boy_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Boy_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Boy_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Boy_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Boy_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Boy_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Boy_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.Girls) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Grl_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Grl_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Grl_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Grl_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Grl_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Grl_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Grl_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Grl_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Grl_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.Disadvantaged) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Dis_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Dis_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Dis_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Dis_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Dis_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Dis_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Dis_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Dis_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Dis_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.NonDisadvantaged) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NDi_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NDi_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NDi_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_NDi_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_NDi_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_NDi_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_NDi_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_NDi_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_NDi_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.Eal) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_EAL_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_EAL_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_EAL_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_EAL_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_EAL_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_EAL_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_EAL_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_EAL_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_EAL_Cohort_Eng_Previous2_Num),
                // "Non-mobile pupils" is not published at LA or England level in the source data -
                // this is expected, not a gap. LA/England always show "no data" when it's selected.
                _ when characteristic.EqualsCaseInsensitive(Ks2ReadingScore.Filters.PupilCharacteristic.Values.NonMobile) => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NMo_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NMo_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_NMo_Cohort_Est_Previous2_Num,
                    x => null, x => null, x => null,
                    x => null, x => null, x => null),
                _ => new(
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.ReadingScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.ReadingScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class AverageScaledScoreMaths
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2MathsScore.Key,
                Ks2MathsScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2MathsScore.Key,
                Ks2MathsScore.Name,
                2024,
                MeasureDataType.ScaledScore,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var characteristic = filters.ContainsKey(Ks2MathsScore.Filters.PupilCharacteristic.Key)
                ? filters[Ks2MathsScore.Filters.PupilCharacteristic.Key]
                : Ks2MathsScore.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2MathsScore.Filters.PupilCharacteristic.Key,
                    Ks2MathsScore.Filters.PupilCharacteristic.Name,
                    Ks2MathsScore.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.Boys) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Boy_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Boy_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Boy_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Boy_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Boy_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Boy_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Boy_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Boy_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Boy_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.Girls) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Grl_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Grl_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Grl_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Grl_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Grl_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Grl_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Grl_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Grl_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Grl_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.Disadvantaged) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Dis_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Dis_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Dis_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Dis_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Dis_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Dis_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Dis_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Dis_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Dis_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.NonDisadvantaged) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NDi_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NDi_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NDi_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_NDi_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_NDi_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_NDi_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_NDi_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_NDi_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_NDi_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.Eal) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_EAL_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_EAL_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_EAL_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_EAL_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_EAL_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_EAL_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_EAL_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_EAL_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_EAL_Cohort_Eng_Previous2_Num),
                // "Non-mobile pupils" is not published at LA or England level in the source data -
                // this is expected, not a gap. LA/England always show "no data" when it's selected.
                _ when characteristic.EqualsCaseInsensitive(Ks2MathsScore.Filters.PupilCharacteristic.Values.NonMobile) => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NMo_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NMo_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_NMo_Cohort_Est_Previous2_Num,
                    x => null, x => null, x => null,
                    x => null, x => null, x => null),
                _ => new(
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.MathsScaledScore_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.MathsScaledScore_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.MathsScaledScore_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class MeetingExpectedStandardGps
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2ExpectedGps.Key,
                Ks2ExpectedGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2ExpectedGps.Key,
                Ks2ExpectedGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var characteristic = filters.ContainsKey(Ks2ExpectedGps.Filters.PupilCharacteristic.Key)
                ? filters[Ks2ExpectedGps.Filters.PupilCharacteristic.Key]
                : Ks2ExpectedGps.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2ExpectedGps.Filters.PupilCharacteristic.Key,
                    Ks2ExpectedGps.Filters.PupilCharacteristic.Name,
                    Ks2ExpectedGps.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.Boys) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_Boy_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Boy_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Boy_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Boy_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Boy_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Boy_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Boy_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Boy_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Boy_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.Girls) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_Grl_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Grl_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Grl_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Grl_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Grl_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Grl_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Grl_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Grl_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Grl_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.Disadvantaged) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_Dis_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Dis_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Dis_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Dis_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Dis_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Dis_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Dis_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Dis_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Dis_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.NonDisadvantaged) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_NDi_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_NDi_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_NDi_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_NDi_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_NDi_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_NDi_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_NDi_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_NDi_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_NDi_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.Eal) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_EAL_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_EAL_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_EAL_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_EAL_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_EAL_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_EAL_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_EAL_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_EAL_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_EAL_Cohort_Eng_Previous2_Num),
                // "Non-mobile pupils" is not published at LA or England level in the source data -
                // this is expected, not a gap. LA/England always show "no data" when it's selected.
                _ when characteristic.EqualsCaseInsensitive(Ks2ExpectedGps.Filters.PupilCharacteristic.Values.NonMobile) => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_NMo_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_NMo_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_NMo_Cohort_Est_Previous2_Num,
                    x => null, x => null, x => null,
                    x => null, x => null, x => null),
                _ => new(
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsExpected_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsExpected_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsExpected_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }

    public static class AchievedHigherStandardGps
    {
        public static Measure ForSchool(SchoolMeasureData<Ks2PerformanceData> currentSchool, IEnumerable<SchoolMeasureData<Ks2PerformanceData>> similarSchools, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchool(
                Ks2HigherGps.Key,
                Ks2HigherGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchools,
                fieldSelector);
        }

        public static Measure ForSchoolComparison(SchoolMeasureData<Ks2PerformanceData> currentSchool, SchoolMeasureData<Ks2PerformanceData> similarSchool, CaseInsensitiveDictionary<string> filters)
        {
            var (availableFilters, fieldSelector) = ResolveFilters(filters);

            return Measure.ForSchoolComparison(
                Ks2HigherGps.Key,
                Ks2HigherGps.Name,
                2024,
                MeasureDataType.GradePercentage,
                availableFilters,
                currentSchool,
                similarSchool,
                fieldSelector);
        }

        private static (IEnumerable<MeasureAvailableFilter> AvailableFilters, MeasureFieldSelector<Ks2PerformanceData> FieldSelector) ResolveFilters(CaseInsensitiveDictionary<string> filters)
        {
            var characteristic = filters.ContainsKey(Ks2HigherGps.Filters.PupilCharacteristic.Key)
                ? filters[Ks2HigherGps.Filters.PupilCharacteristic.Key]
                : Ks2HigherGps.Filters.PupilCharacteristic.Values.AllPupils;

            IEnumerable<MeasureAvailableFilter> availableFilters = [
                new MeasureAvailableFilter(
                    Ks2HigherGps.Filters.PupilCharacteristic.Key,
                    Ks2HigherGps.Filters.PupilCharacteristic.Name,
                    Ks2HigherGps.Filters.PupilCharacteristic.Values.AllValues.Select(f =>
                        new FilterOption(f.Value, f.Name, f.Value.EqualsCaseInsensitive(characteristic)))
                    .ToList())
            ];

            MeasureFieldSelector<Ks2PerformanceData> fieldSelector = characteristic switch
            {
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.Boys) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_Boy_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Boy_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Boy_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Boy_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Boy_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Boy_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Boy_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Boy_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Boy_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.Girls) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_Grl_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Grl_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Grl_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Grl_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Grl_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Grl_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Grl_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Grl_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Grl_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.Disadvantaged) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_Dis_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Dis_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Dis_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Dis_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Dis_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Dis_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Dis_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Dis_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Dis_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.NonDisadvantaged) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_NDi_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_NDi_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_NDi_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_NDi_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_NDi_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_NDi_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_NDi_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_NDi_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_NDi_Cohort_Eng_Previous2_Num),
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.Eal) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_EAL_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_EAL_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_EAL_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_EAL_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_EAL_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_EAL_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_EAL_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_EAL_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_EAL_Cohort_Eng_Previous2_Num),
                // "Non-mobile pupils" is not published at LA or England level in the source data -
                // this is expected, not a gap. LA/England always show "no data" when it's selected.
                _ when characteristic.EqualsCaseInsensitive(Ks2HigherGps.Filters.PupilCharacteristic.Values.NonMobile) => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_NMo_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_NMo_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_NMo_Cohort_Est_Previous2_Num,
                    x => null, x => null, x => null,
                    x => null, x => null, x => null),
                _ => new(
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Current_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous_Num,
                    x => x?.EstablishmentPerformance?.GpsHigher_Tot_Cohort_Est_Previous2_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Current_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous_Num,
                    x => x?.LocalAuthorityPerformance?.GpsHigher_Tot_Cohort_LA_Previous2_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Current_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous_Num,
                    x => x?.EnglandPerformance?.GpsHigher_Tot_Cohort_Eng_Previous2_Num)
            };

            return (availableFilters, fieldSelector);
        }
    }
}
