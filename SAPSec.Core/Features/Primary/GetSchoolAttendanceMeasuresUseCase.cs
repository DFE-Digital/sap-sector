using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetAttendanceMeasuresRequest, GetAttendanceMeasuresResponse>
{
    public async Task<GetAttendanceMeasuresResponse> Execute(GetAttendanceMeasuresRequest request)
    {
        var dataProvider = new PrimaryAttendanceMeasuresDataProvider(
              absenceRepository,
              establishmentRepository);

        var currentSchoolPerformance = await dataProvider.GetSchoolAttendance(request.Urn);

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolPerformance.SchoolInfo,
            AttendanceMeasures.TotalAbsence.ForSchool(
                currentSchoolPerformance,
                filterBy));
    }

}

public record GetAttendanceMeasuresRequest(
string Urn,
IDictionary<string, string>? FilterBy = null);

public record GetAttendanceMeasuresResponse(
SchoolInfo.SchoolInfo School,
Measure Absence);
