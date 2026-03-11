using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.KS4.Performance;

namespace SAPSec.Infrastructure.Json;

public class JsonKs4PerformanceRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentPerformance> establishmentPerformanceRepository,
    IJsonFile<LAPerformance> localAuthorityPerformanceRepository,
    IJsonFile<EnglandPerformance> englandPerformanceRepository) : IKs4PerformanceRepository
{
    public async Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
        if (string.IsNullOrWhiteSpace(establishment?.URN))
        {
            return null;
        }

        var establishmentPerformance = (await establishmentPerformanceRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == urn);

        var localAuthorityPerformance = (await localAuthorityPerformanceRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == establishment.LAId);

        var englandPerformance = (await englandPerformanceRepository.ReadAllAsync())
            .FirstOrDefault();

        return new(
            establishmentPerformance,
            localAuthorityPerformance,
            englandPerformance);
    }
}
