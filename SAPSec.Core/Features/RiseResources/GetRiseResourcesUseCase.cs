using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

public class GetRiseResourcesUseCase(
    IEstablishmentRepository establishmentRepository)
    : IUseCase<GetRiseResourcesRequest, GetRiseResourcesResponse>
{
    public async Task<GetRiseResourcesResponse> Execute(GetRiseResourcesRequest request)
    {
        var dataProvider = new RiseResourcesDataProvider(establishmentRepository);

        var data = await dataProvider.GetRiseResourcesData(request.Urn);

        return new(
            Urn: data.Establishment.URN,
            SchoolName: data.Establishment.EstablishmentName,
            Resources: data.Resources,
            LastUpdated: data.LastUpdated);
    }
}

public record GetRiseResourcesRequest(string Urn);

public record GetRiseResourcesResponse(
    string Urn,
    string SchoolName,
    IReadOnlyList<RiseResource> Resources,
    DateTime? LastUpdated);

public record RiseResource(
    string Title,
    string? Description = null,
    string? Url = null,
    string? Category = null,
    IReadOnlyList<string>? Tags = null);
