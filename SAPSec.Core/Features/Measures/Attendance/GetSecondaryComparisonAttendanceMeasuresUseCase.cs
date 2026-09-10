using SAPSec.Core.Extensions;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures.Attendance;

public class GetSecondaryComparisonAttendanceMeasuresUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<GetSecondaryComparisonAttendanceMeasuresRequest, GetComparisonAttendanceMeasuresResponse>
{
    public async Task<GetComparisonAttendanceMeasuresResponse> Execute(GetSecondaryComparisonAttendanceMeasuresRequest request)
    {
        var dataProvider = new ComparisonMeasureDataProvider<AbsenceData, SimilarSchoolsSecondaryGroupsEntry, SimilarSchoolsSecondaryValuesEntry>(
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
                MeasurePhase.Secondary,
                currentSchoolData,
                comparatorSchoolData,
                filterBy));
    }
}

public record GetSecondaryComparisonAttendanceMeasuresRequest(
    string CurrentSchoolUrn,
    string ComparatorSchoolUrn,
    IDictionary<string, string>? FilterBy = null);
