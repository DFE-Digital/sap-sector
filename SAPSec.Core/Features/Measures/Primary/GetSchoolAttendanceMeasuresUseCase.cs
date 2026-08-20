using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Primary;

public class GetSchoolAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse>
{
    public async Task<GetSchoolAttendanceMeasuresResponse> Execute(GetSchoolAttendanceMeasuresRequest request)
    {
        var dataProvider = new SchoolAbsenceDataProvider(
              absenceRepository,
              establishmentRepository);

        var currentSchoolPerformance = await dataProvider.GetSchoolAttendance(request.Urn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolPerformance.SchoolInfo,
            AttendanceMeasures.Absence.ForSchool(
                currentSchoolPerformance,
                filterBy));
    }

}

public record GetSchoolAttendanceMeasuresRequest(
string Urn,
IDictionary<string, string>? FilterBy = null);

public record GetSchoolAttendanceMeasuresResponse(
SchoolInfo.SchoolInfo School,
Measure Absence);
