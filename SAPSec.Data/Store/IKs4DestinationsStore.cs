using SAPSec.Data.Dto.KS4.Destinations;

namespace SAPSec.Data.Store;

public interface IKs4DestinationsStore
{
    Task<Ks4DestinationsData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4DestinationsData(
    string Urn,
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations);
