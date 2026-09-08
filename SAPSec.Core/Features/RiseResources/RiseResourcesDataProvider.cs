using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.RiseResources;

internal class RiseResourcesDataProvider(
    IEstablishmentRepository establishmentRepository)
{
    public async Task<RiseResourcesSourceData> GetRiseResourcesData(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn)
            ?? throw new NotFoundException($"School with URN {urn} was not found");

        var resources = Array.Empty<RiseResource>();

        return new RiseResourcesSourceData(establishment, resources, LastUpdated: null);
    }
}

internal record RiseResourcesSourceData(
    Establishment Establishment,
    IReadOnlyList<RiseResource> Resources,
    DateTime? LastUpdated);
