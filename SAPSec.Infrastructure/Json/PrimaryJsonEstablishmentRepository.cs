using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class PrimaryJsonEstablishmentRepository : IEstablishmentRepository
{
    private readonly IJsonFile<Establishment> _establishmentJsonFile;
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailJsonFile;
    private readonly ILogger<PrimaryJsonEstablishmentRepository> _logger;

    public PrimaryJsonEstablishmentRepository(
        IJsonFileFactory jsonFileFactory,
        ILogger<PrimaryJsonEstablishmentRepository> logger)
    {
        _establishmentJsonFile = jsonFileFactory.Create<Establishment>(JsonDataSource.PrimarySchools);
        _establishmentEmailJsonFile = jsonFileFactory.Create<EstablishmentEmail>(JsonDataSource.PrimarySchools);
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        var establishments = await _establishmentJsonFile.ReadAllAsync();
        return establishments.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        var urnSet = urns
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.Ordinal);

        if (urnSet.Count == 0)
        {
            return [];
        }

        var establishments = await _establishmentJsonFile.ReadAllAsync();
        return establishments.Where(e => urnSet.Contains(e.URN)).ToList().AsReadOnly();
    }

    public async Task<Establishment?> GetEstablishmentAsync(string urn)
    {
        var establishments = await GetEstablishmentsAsync([urn]);
        return establishments.FirstOrDefault();
    }

    public async Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
    {
        var allEstablishments = await _establishmentJsonFile.ReadAllAsync();
        return allEstablishments.FirstOrDefault(x => x.URN == number || x.UKPRN == number || x.LAESTAB == number);
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        var establishmentEmails = await _establishmentEmailJsonFile.ReadAllAsync();
        return establishmentEmails.FirstOrDefault(x => x.URN == urn);
    }
}
