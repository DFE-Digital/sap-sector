using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;

namespace SAPSec.Infrastructure.Repositories.Json;

public class JsonKs4PerformanceRepository(
    IJsonFile<Establishment> establishmentRepository,
    IJsonFile<EstablishmentPerformance> establishmentPerformanceRepository,
    IJsonFile<LAPerformance> localAuthorityPerformanceRepository,
    IJsonFile<EnglandPerformance> englandPerformanceRepository) : IKs4PerformanceRepository
{
    public async Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn)
    {
        var establishments = await establishmentRepository.ReadAllAsync();
        var establishment = establishments.FirstOrDefault(e => e.URN == urn);
        if (establishment is null)
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
