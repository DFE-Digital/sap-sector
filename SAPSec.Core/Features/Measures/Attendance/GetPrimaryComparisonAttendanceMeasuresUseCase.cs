using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class GetPrimaryComparisonAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetPrimaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse>
{
    public async Task<GetComparisonAttendanceMeasuresResponse> Execute(GetPrimaryComparisonAttendanceMeasuresRequest request)
    {
        var dataProvider = new ComparisonMeasureDataProvider<AbsenceData, SimilarSchoolsPrimaryGroupsEntry, SimilarSchoolsPrimaryValuesEntry>(
            establishmentRepository,
            similarSchoolsRepository,
            absenceRepository);

        var (currentSchoolData, comparatorSchoolData) = await dataProvider.GetData(
            request.CurrentSchoolUrn,
            request.ComparatorSchoolUrn);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            currentSchoolData.SchoolInfo,
            comparatorSchoolData.SchoolInfo,
            AttendanceMeasures.Absence.ForSchoolComparison(
                MeasurePhase.Primary,
                currentSchoolData,
                comparatorSchoolData,
                filterBy));
    }
}

public record GetPrimaryComparisonAttendanceMeasuresRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);
