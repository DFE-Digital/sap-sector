using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class GetSchoolAllThroughAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetSchoolAllThroughAttendanceMeasuresRequest, GetSchoolAllThroughAttendanceMeasuresResponse>
{
    public async Task<GetSchoolAllThroughAttendanceMeasuresResponse> Execute(GetSchoolAllThroughAttendanceMeasuresRequest request)
    {
        var dataProvider = new SchoolMeasureDataProvider<AbsenceData>(
              establishmentRepository,
              absenceRepository);

        var currentSchoolPerformance = await dataProvider.GetData(request.Urn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            AttendanceMeasures.Absence.ForSchool(
                MeasurePhase.Primary,
                currentSchoolPerformance,
                filterBy,
                MeasurePhase.Primary.KeyPrefix()),
            AttendanceMeasures.Absence.ForSchool(
                MeasurePhase.Secondary,
                currentSchoolPerformance,
                filterBy,
                MeasurePhase.Secondary.KeyPrefix()));
    }
}

public record GetSchoolAllThroughAttendanceMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolAllThroughAttendanceMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    Measure PrimaryAbsence,
    Measure SecondaryAbsence);
