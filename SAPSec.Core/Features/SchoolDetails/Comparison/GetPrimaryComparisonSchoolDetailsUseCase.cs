using SAPSec.Core.Features.Geography;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SchoolDetails.Comparison;

public class GetPrimaryComparisonSchoolDetailsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    ISchoolDetailsService schoolDetailsService)
    : IUseCase<GetComparisonSchoolDetailsRequest, GetComparisonSchoolDetailsResponse>
{
    public async Task<GetComparisonSchoolDetailsResponse> Execute(GetComparisonSchoolDetailsRequest request)
    {
        var dataProvider = new ComparisonSchoolDetailsDataProvider<SimilarSchoolsPrimaryGroupsEntry, SimilarSchoolsPrimaryValuesEntry>(establishmentRepository, similarSchoolsRepository);

        var (currentSchool, comparatorSchool) = await dataProvider.GetData(request.CurrentSchoolUrn, request.ComparatorSchoolUrn);

        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.ComparatorSchoolUrn);

        return new(
            new SchoolWithCoordinates(currentSchool.SchoolInfo, currentSchool.Data.ToGeographicCoordinates()),
            new SchoolWithCoordinates(comparatorSchool.SchoolInfo, comparatorSchool.Data.ToGeographicCoordinates()),
            currentSchool.Data?.DistanceMiles(comparatorSchool.Data),
            similarSchoolDetails);
    }
}
