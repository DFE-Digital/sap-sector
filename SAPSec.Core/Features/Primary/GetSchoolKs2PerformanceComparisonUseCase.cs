using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Primary;

public class GetSchoolKs2PerformanceComparisonUseCase(
    IEstablishmentRepository establishmentRepository,
    IKs2PerformanceRepository performanceRepository)
    : IUseCase<GetSchoolKs2PerformanceComparisonRequest, GetSchoolKs2PerformanceComparisonResponse>
{
    public async Task<GetSchoolKs2PerformanceComparisonResponse> Execute(GetSchoolKs2PerformanceComparisonRequest request)
    {
        var urns = new[] { request.Urn, request.SimilarSchoolUrn };

        var schools = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .Select(SchoolInfo.SchoolInfo.FromEstablishment)
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        if (!schools.ContainsKey(request.Urn))
        {
            throw new NotFoundException($"School not found with URN: {request.Urn}");
        }

        if (!schools.ContainsKey(request.SimilarSchoolUrn))
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var performances = (await performanceRepository.GetByUrnsAsync(urns))
            .ToDictionary(x => x.Urn, StringComparer.Ordinal);

        var currentSchoolData = new SchoolData<Ks2PerformanceData>(
            schools[request.Urn],
            performances.TryGetValue(request.Urn, out var currentPerformance) ? currentPerformance : null);

        var similarSchoolData = new SchoolData<Ks2PerformanceData>(
            schools[request.SimilarSchoolUrn],
            performances.TryGetValue(request.SimilarSchoolUrn, out var similarPerformance) ? similarPerformance : null);

        var filterBy = request.FilterBy.AsCaseInsensitive();

        return new(
            Ks2PerformanceMeasures.MeetingExpectedStandardRwm.ForSchoolComparison(
                currentSchoolData,
                similarSchoolData,
                [],
                filterBy));
    }
}

public record GetSchoolKs2PerformanceComparisonRequest(
    string Urn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs2PerformanceComparisonResponse(
    Measure MeetingExpectedStandardRwm);
