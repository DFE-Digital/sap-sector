using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchools(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository)
{
    public async Task<FindPrimarySimilarSchoolsResponse> Execute(FindPrimarySimilarSchoolsRequest request)
    {
        var groups = await similarSchoolsRepository.GetGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]).Distinct().ToArray();

        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);
        var values = SimilarSchoolsPrimaryValues.FromData(await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
            .ToDictionary(v => v.Urn, v => v);

        var currentEstablishment = establishments.FirstOrDefault(e => e.URN == request.CurrentSchoolUrn);
        if (currentEstablishment is null)
        {
            throw new NotFoundException($"School with URN {request.CurrentSchoolUrn} was not found");
        }

        if (!values.TryGetValue(request.CurrentSchoolUrn, out var currentValues))
        {
            throw new NotFoundException($"No similar schools characteristics found for URN {request.CurrentSchoolUrn}");
        }

        var establishmentsByUrn = establishments.ToDictionary(e => e.URN, e => e);

        var similarSchools = groups
            .Select(group =>
            {
                if (!establishmentsByUrn.TryGetValue(group.NeighbourURN, out var establishment))
                {
                    return null;
                }

                if (!values.TryGetValue(group.NeighbourURN, out var similarValues))
                {
                    return null;
                }

                return new PrimarySimilarSchool(
                    group.NeighbourURN,
                    establishment.EstablishmentName,
                    establishment.LAName,
                    group.Rank,
                    group.Dist,
                    ToCharacteristics(similarValues));
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList()
            .AsReadOnly();

        return new(
            new PrimaryCurrentSchool(
                currentEstablishment.URN,
                currentEstablishment.EstablishmentName,
                currentEstablishment.LAName,
                ToCharacteristics(currentValues)),
            similarSchools);
    }

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

public record FindPrimarySimilarSchoolsRequest(string CurrentSchoolUrn);

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
