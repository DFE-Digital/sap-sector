using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.Destinations;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4PerformanceRepository
{
    Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn);
}

public record Ks4HeadlineMeasuresData(
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance,
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations);
