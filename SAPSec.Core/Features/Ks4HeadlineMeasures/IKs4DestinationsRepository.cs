using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4DestinationsRepository
{
    Task<Ks4DestinationsData?> GetByUrnAsync(string urn);
}

public record Ks4DestinationsData(
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations);
