using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

public class GetRiseResourcesUseCase(
    IEstablishmentRepository establishmentRepository)
    : IUseCase<GetRiseResourcesRequest, GetRiseResourcesResponse>
{
    public async Task<GetRiseResourcesResponse> Execute(GetRiseResourcesRequest request)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(request.Urn) ?? throw new NotFoundException($"School with URN {request.Urn} was not found");

        // TODO: Implement logic to fetch RISE resources from JSON or data source
        // For now, return empty list to maintain contract
        var resources = Array.Empty<RiseResource>();

        return new(
            Urn: establishment.URN,
            SchoolName: establishment.EstablishmentName,
            Resources: resources,
            LastUpdated: null);
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
