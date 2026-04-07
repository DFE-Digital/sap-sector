using SAPSec.Data.Model.Generated;

namespace SAPSec.Data;

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
