using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchools(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository)
    : IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse>
{
    public async Task<FindPrimarySimilarSchoolsResponse> Execute(FindPrimarySimilarSchoolsRequest request)
    {
        var dataProvider = new PrimarySimilarSchoolsCharacteristicsDataProvider(
            establishmentRepository,
            similarSchoolsRepository);

        var (currentSchool, similarSchools) = await dataProvider.GetSimilarSchoolsCharacteristics(request.Urn);

        return new(
            ToCurrentSchool(currentSchool),
            similarSchools.Select(ToSimilarSchool).ToList().AsReadOnly());
    }

    private static PrimaryCurrentSchool ToCurrentSchool(SchoolData<SimilarSchoolsPrimaryValues> school) =>
        new(
            school.SchoolInfo.Urn,
            school.SchoolInfo.Name,
            school.SchoolInfo.LocalAuthority.Name,
            ToCharacteristics(school.Data!));

    private static PrimarySimilarSchool ToSimilarSchool(RankedSchoolData<SimilarSchoolsPrimaryValues> school) =>
        new(
            school.School.SchoolInfo.Urn,
            school.School.SchoolInfo.Name,
            school.School.SchoolInfo.LocalAuthority.Name,
            school.Rank,
            school.Distance,
            ToCharacteristics(school.School.Data!));

    private static PrimarySimilarSchoolCharacteristics ToCharacteristics(SimilarSchoolsPrimaryValues values) =>
        new(
            values.ReadMatAverage,
            values.Ks1PriorRwmAverage,
            values.PupilPremiumEligibilityPercentage,
            values.PupilsWithEalPercentage,
            values.Polar4Quintile,
            values.PupilStabilityRate,
            values.AverageIdaciScore,
            values.PupilsWithSenSupportPercentage,
            values.PupilCount,
            values.PupilsWithEhcPlanPercentage);
}

public record FindPrimarySimilarSchoolsRequest(string Urn);

public record FindPrimarySimilarSchoolsResponse(
    PrimaryCurrentSchool CurrentSchool,
    IReadOnlyCollection<PrimarySimilarSchool> SimilarSchools);

public record PrimaryCurrentSchool(
    string Urn,
    string Name,
    string LocalAuthorityName,
    PrimarySimilarSchoolCharacteristics Characteristics);

public record PrimarySimilarSchool(
    string Urn,
    string Name,
    string LocalAuthorityName,
    string Rank,
    string Distance,
    PrimarySimilarSchoolCharacteristics Characteristics);

public record PrimarySimilarSchoolCharacteristics(
    decimal ReadMatAverage,
    decimal Ks1PriorRwmAverage,
    decimal PupilPremiumEligibilityPercentage,
    decimal PupilsWithEalPercentage,
    decimal Polar4Quintile,
    decimal PupilStabilityRate,
    decimal AverageIdaciScore,
    decimal PupilsWithSenSupportPercentage,
    decimal PupilCount,
    decimal PupilsWithEhcPlanPercentage);
