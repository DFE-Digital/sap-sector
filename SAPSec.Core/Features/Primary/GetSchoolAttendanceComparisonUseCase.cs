using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolAttendanceComparisonUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetSchoolAttendanceComparisonRequest, GetSchoolAttendanceComparisonResponse>
{
    public async Task<GetSchoolAttendanceComparisonResponse> Execute(GetSchoolAttendanceComparisonRequest request)
    {
        var dataProvider = new PrimaryAttendanceComparisonDataProvider(
            establishmentRepository,
            absenceRepository);

        var (currentSchoolData, similarSchoolData) = await dataProvider.GetComparisonAttendance(
            request.Urn,
            request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            AttendanceMeasures.Absence.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy));
    }
}

public record GetSchoolAttendanceComparisonRequest(
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolAttendanceComparisonResponse(
    Measure Absence);
