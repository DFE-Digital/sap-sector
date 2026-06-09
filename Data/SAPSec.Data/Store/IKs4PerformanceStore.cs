using SAPSec.Data.Dto;

namespace SAPSec.Data.Store;

public interface IKs4PerformanceStore
{
    Task<Ks4PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4PerformanceData(
    string URN,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

