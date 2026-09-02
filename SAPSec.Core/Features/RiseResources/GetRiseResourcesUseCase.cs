using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

public class GetRiseResourcesUseCase(
    IEstablishmentRepository establishmentRepository,
    IRiseResourcesRepository riseResourcesRepository)
    : IUseCase<GetRiseResourcesRequest, GetRiseResourcesResponse>
{
    public async Task<GetRiseResourcesResponse> Execute(GetRiseResourcesRequest request)
    {
        var dataProvider = new RiseResourcesDataProvider(establishmentRepository, riseResourcesRepository);

        var data = await dataProvider.GetRiseResourcesData(request.Urn);

        return new(
            Urn: data.Establishment.URN,
            SchoolName: data.Establishment.EstablishmentName,
            Resources: data.Resources);
    }
}

public record GetRiseResourcesRequest(string Urn);

public record GetRiseResourcesResponse(
    string Urn,
    string SchoolName,
    IReadOnlyList<RiseResource> Resources);

public record RiseResource(
    string Title,
    string? Description = null,
    string? Url = null,
    string? Category = null,
    string? SubCategory = null,
    IReadOnlyList<string>? MappingMeasures = null);
