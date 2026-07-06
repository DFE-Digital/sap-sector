using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Data.Store;

public interface IKs2PerformanceStore
{
    Task<Ks2PerformanceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<Ks2PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns);

    //Task<EstablishmentPerformance?> GetEstablishmentByUrnAsync(string urn);
    //Task<IReadOnlyCollection<EstablishmentPerformance>> GetEstablishmentByUrnsAsync(IEnumerable<string> urns);
    //Task<LAPerformance?> GetLAByIdAsync(string laId);
    //Task<IReadOnlyCollection<LAPerformance>> GetLAByIdsAsync(IEnumerable<string> laId);
    //Task<EnglandPerformance?> GetEnglandAsync();

}

public record Ks2PerformanceData(
    string Urn,
    EstablishmentPerformance? EstablishmentPerformance,
    LAPerformance? LocalAuthorityPerformance,
    EnglandPerformance? EnglandPerformance);

