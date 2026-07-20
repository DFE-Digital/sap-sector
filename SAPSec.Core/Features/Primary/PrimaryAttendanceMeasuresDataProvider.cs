using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class PrimaryAttendanceMeasuresDataProvider(
    IAbsenceRepository attendanceRepository,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsPrimaryRepository
   /* ISimilarSchoolsSecondaryRepository similarSchoolsSecondaryRepository*/)
{
    public async Task<SimilarSchoolsData<AbsenceData>> GetSimilarSchoolsAttendance(string currentSchoolUrn, string phase)
    {
        var similarSchoolUrns = new string[0];

        //if (phase == PhaseOfEducationValues.Secondary)
        //{
        //    similarSchoolUrns = (await similarSchoolsSecondaryRepository.GetGroupAsync(currentSchoolUrn))
        //       .Select(x => x.NeighbourURN)
        //       .Where(x => !string.IsNullOrWhiteSpace(x))
        //       .Distinct(StringComparer.Ordinal)
        //       .ToArray();
        //}
        //else
        //{
        similarSchoolUrns = (await similarSchoolsPrimaryRepository.GetGroupAsync(currentSchoolUrn))
            .Select(x => x.NeighbourURN)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        //}

        var schools = (await establishmentRepository.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(currentSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        }

        var currentSchool = schools[currentSchoolUrn];

        var attendances = (await attendanceRepository.GetByUrnsAsync(schools.Keys))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<AbsenceData>(
            currentSchool,
            attendances[currentSchoolUrn]);

        var similarSchoolsData = similarSchoolUrns
            .Where(schools.ContainsKey)
            .Select(urn => new SchoolData<AbsenceData>(
                schools[urn],
                attendances.TryGetValue(urn, out var p) ? p : null))
            .ToList();

        return new SimilarSchoolsData<AbsenceData>(
         currentSchoolData,
         similarSchoolsData);

        //var similarSchoolData = similarSchoolUrns.Length == 0
        //    ? Array.Empty<AbsenceData>()
        //    : await repository.GetByUrnsAsync(similarSchoolUrns);

        //var similarSchoolDataByUrn = similarSchoolData
        //    .Where(x => !string.IsNullOrWhiteSpace(x.Urn))
        //    .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        //var similarSchoolDetails = similarSchoolUrns.Length == 0
        //    ? Array.Empty<Establishment>()
        //    : (await establishmentRepository.GetEstablishmentsAsync(similarSchoolUrns))
        //        ?? Array.Empty<Establishment>();

        //var similarSchoolDetailsByUrn = similarSchoolDetails
        //    .Where(x => !string.IsNullOrWhiteSpace(x.URN))
        //    .ToDictionary(x => x.URN, StringComparer.Ordinal);

        //var similarSchoolMeasures = similarSchoolUrns
        //    .Where(similarSchoolDetailsByUrn.ContainsKey)
        //    .Select(urn => new SimilarSchoolAttendanceMeasure(
        //        urn,
        //        similarSchoolDetailsByUrn[urn].EstablishmentName,
        //        similarSchoolDataByUrn.GetValueOrDefault(urn)))
        //    .ToArray();

        //var overallSchoolSeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct),
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct),
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct));
        //var persistentSchoolSeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct),
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct),
        //    ParseNullableDecimal(data?.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct));

        //var overallLocalAuthoritySeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Current_Pct),
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous_Pct),
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Tot_LA_Previous2_Pct));
        //var persistentLocalAuthoritySeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Current_Pct),
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous_Pct),
        //    ParseNullableDecimal(data?.LocalAuthorityAbsence?.Abs_Persistent_LA_Previous2_Pct));

        //var overallEnglandSeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Current_Pct),
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous_Pct),
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Tot_Eng_Previous2_Pct));
        //var persistentEnglandSeries = new AttendanceMeasureSeries(
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Current_Pct),
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous_Pct),
        //    ParseNullableDecimal(data?.EnglandAbsence?.Abs_Persistent_Eng_Previous2_Pct));
        //var overallSimilarSchoolsSeries = new AttendanceMeasureSeries(
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct))),
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous_Pct))),
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Previous2_Pct))));
        //var persistentSimilarSchoolsSeries = new AttendanceMeasureSeries(
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct))),
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous_Pct))),
        //    AverageAvailable(similarSchoolData.Select(x => ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Previous2_Pct))));

        //return new(
        //    new AttendanceMeasureAverage(
        //        overallSchoolSeries.Current,
        //        AverageAvailable(similarSchoolData.Select(x =>
        //            ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct))),
        //        overallLocalAuthoritySeries.Current,
        //        overallEnglandSeries.Current),
        //    BuildTopPerformers(
        //        establishment,
        //        overallSchoolSeries.Current,
        //        similarSchoolMeasures,
        //        x => ParseNullableDecimal(x.AbsenceData?.EstablishmentAbsence?.Abs_Tot_Est_Current_Pct)),
        //    new AttendanceMeasureYearByYear(
        //        overallSchoolSeries,
        //        overallSimilarSchoolsSeries,
        //        overallLocalAuthoritySeries,
        //        overallEnglandSeries),
        //    new AttendanceMeasureAverage(
        //        persistentSchoolSeries.Current,
        //        AverageAvailable(similarSchoolData.Select(x =>
        //            ParseNullableDecimal(x.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct))),
        //        persistentLocalAuthoritySeries.Current,
        //        persistentEnglandSeries.Current),
        //    BuildTopPerformers(
        //        establishment,
        //        persistentSchoolSeries.Current,
        //        similarSchoolMeasures,
        //        x => ParseNullableDecimal(x.AbsenceData?.EstablishmentAbsence?.Abs_Persistent_Est_Current_Pct)),
        //    new AttendanceMeasureYearByYear(
        //        persistentSchoolSeries,
        //        persistentSimilarSchoolsSeries,
        //        persistentLocalAuthoritySeries,
        //        persistentEnglandSeries));



        //var similarSchoolUrns = (await similarSchoolsRepository.GetGroupAsync(currentSchoolUrn))
        //    .Select(g => g.NeighbourURN)
        //    .Where(urn => !string.IsNullOrWhiteSpace(urn))
        //    .Distinct(StringComparer.Ordinal)
        //    .ToArray();

        //var schools = (await establishmentRepository.GetEstablishmentsAsync([currentSchoolUrn, .. similarSchoolUrns]))
        //    .Select(SchoolInfo.SchoolInfo.FromEstablishment)
        //    .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        //if (!schools.ContainsKey(currentSchoolUrn))
        //{
        //    throw new NotFoundException($"School not found with URN: {currentSchoolUrn}");
        //}

        //var currentSchool = schools[currentSchoolUrn];

        //var performances = (await performanceRepository.GetByUrnsAsync(schools.Keys))
        //    .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        //var currentSchoolData = new SchoolData<Ks2PerformanceData>(
        //    currentSchool,
        //    performances[currentSchoolUrn]);

        //var similarSchoolsData = similarSchoolUrns
        //    .Where(schools.ContainsKey)
        //    .Select(urn => new SchoolData<Ks2PerformanceData>(
        //        schools[urn],
        //        performances.TryGetValue(urn, out var p) ? p : null))
        //    .ToList();

        //return new SimilarSchoolsData<Ks2PerformanceData>(
        //    currentSchoolData,
        //    similarSchoolsData);
    }
}
