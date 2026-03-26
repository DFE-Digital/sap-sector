using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonEstablishmentRepository : IEstablishmentRepository
{
    private readonly IJsonFile<Establishment> _establishmentJsonFile;
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailJsonFile;

    private ILogger<Establishment> _logger;

    public JsonEstablishmentRepository(
        IJsonFile<Establishment> establishmentJsonFile,
        IJsonFile<EstablishmentEmail> establishmentEmailJsonFile,
        ILogger<Establishment> logger)
    {
        _establishmentJsonFile = establishmentJsonFile;
        _establishmentEmailJsonFile = establishmentEmailJsonFile;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        var establishments = await _establishmentJsonFile.ReadAllAsync();

        return establishments.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        var establishments = await _establishmentJsonFile.ReadAllAsync();

        return establishments.Where(e => urns.Contains(e.URN)).ToList().AsReadOnly();
    }

    public async Task<Establishment?> GetEstablishmentAsync(string urn)
    {
        var allEstablishments = await GetEstablishmentsAsync([urn]);
        var establishment = allEstablishments.FirstOrDefault();

        return establishment;
    }

    public async Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
    {
        var allEstablishments = await _establishmentJsonFile.ReadAllAsync();

        return allEstablishments.FirstOrDefault(x => x.URN == number || x.UKPRN == number || $"{x.LAId}{x.EstablishmentNumber}" == number);
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        var establishmentEmails = await _establishmentEmailJsonFile.ReadAllAsync();

        return establishmentEmails.FirstOrDefault(x => x.URN == urn);
    }

}
