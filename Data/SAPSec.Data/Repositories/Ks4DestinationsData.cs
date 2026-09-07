using SAPSec.Data.Dto.KS4.Destinations;

namespace SAPSec.Data.Repositories;

public record Ks4DestinationsData(
    string Urn,
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations) : IMeasureData;
