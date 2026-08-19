using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetComparisonAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse>
{
    public async Task<GetComparisonAttendanceMeasuresResponse> Execute(GetComparisonAttendanceMeasuresRequest request)
    {
        var dataProvider = new PrimaryAbsenceComparisonDataProvider(
            establishmentRepository,
            absenceRepository);

        var (currentSchoolData, similarSchoolData) = await dataProvider.GetComparisonAbsence(
            request.CurrentSchoolUrn,
            request.SimilarSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolData.SchoolInfo,
            similarSchoolData.SchoolInfo,
            AttendanceMeasures.Absence.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                filterBy));
    }
}

public record GetComparisonAttendanceMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetComparisonAttendanceMeasuresResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    SchoolInfo.SchoolInfo SimilarSchool,
    Measure Absence);
