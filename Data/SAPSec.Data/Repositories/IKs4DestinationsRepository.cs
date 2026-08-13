using SAPSec.Data.Dto.KS4.Destinations;

namespace SAPSec.Data.Repositories;

public interface IKs4DestinationsRepository
{
    Task<Ks4DestinationsData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4DestinationsData(
    string Urn,
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations);
