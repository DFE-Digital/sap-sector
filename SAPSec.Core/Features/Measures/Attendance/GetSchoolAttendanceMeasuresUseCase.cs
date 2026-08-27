using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class GetSchoolAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse>
{
    public async Task<GetSchoolAttendanceMeasuresResponse> Execute(GetSchoolAttendanceMeasuresRequest request)
    {
        var dataProvider = new SchoolMeasureDataProvider<AbsenceData>(
              establishmentRepository,
              absenceRepository);

        var currentSchoolPerformance = await dataProvider.GetData(request.Urn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            AttendanceMeasures.Absence.ForSchool(
                request.Phase,
                currentSchoolPerformance,
                filterBy));
    }

}

public record GetSchoolAttendanceMeasuresRequest(
    MeasurePhase Phase,
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolAttendanceMeasuresResponse(
    SchoolInfo.SchoolInfo School,
    Measure Absence);
