using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public interface IKs4PerformanceRepository
{
    Task<Ks4PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

