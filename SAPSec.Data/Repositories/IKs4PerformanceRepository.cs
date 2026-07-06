using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Data.Repositories;

public interface IKs4PerformanceRepository
{
    Task<Ks4PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks4PerformanceData(
    string URN,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

