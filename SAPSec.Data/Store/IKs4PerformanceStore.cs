using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Data.Store;

public interface IKs4PerformanceStore
{
    Task<Ks4PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

