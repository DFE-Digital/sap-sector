using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonKs4DestinationsRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentDestinations> establishmentDestinationsRepository,
    IJsonFile<LADestinations> localAuthorityDestinationsRepository,
    IJsonFile<EnglandDestinations> englandDestinationsRepository) : IKs4DestinationsRepository
{
    public async Task<Ks4DestinationsData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
        if (string.IsNullOrWhiteSpace(establishment?.URN))
        {
            return null;
        }

        var establishmentDestinations = (await establishmentDestinationsRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == urn);

        var localAuthorityDestinations = (await localAuthorityDestinationsRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == establishment.LAId);

        var englandDestinations = (await englandDestinationsRepository.ReadAllAsync())
            .FirstOrDefault();

        return new(
            establishmentDestinations,
            localAuthorityDestinations,
            englandDestinations);
    }
}
