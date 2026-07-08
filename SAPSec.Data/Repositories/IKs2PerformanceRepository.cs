using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Data.Repositories;

public interface IKs2PerformanceRepository
{
    Task<Ks2PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks2PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record Ks2PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);