using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4PerformanceRepository
{
    Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn);
    Task<IReadOnlyDictionary<string, Ks4HeadlineMeasuresData?>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4HeadlineMeasuresData(
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance,
    EstablishmentDestinations? EstablishmentDestinations,
    LADestinations? LocalAuthorityDestinations,
    EnglandDestinations? EnglandDestinations);
