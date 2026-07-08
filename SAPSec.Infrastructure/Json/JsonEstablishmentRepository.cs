using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonEstablishmentRepository : IEstablishmentRepository
{
    private readonly IJsonFile<Establishment> _establishmentFile;
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailFile;

    private ILogger<JsonEstablishmentRepository> _logger;

    public JsonEstablishmentRepository(
        IJsonFile<Establishment> establishmentFile,
        IJsonFile<EstablishmentEmail> establishmentEmailFile,
        ILogger<JsonEstablishmentRepository> logger)
    {
        _establishmentFile = establishmentFile;
        _establishmentEmailFile = establishmentEmailFile;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        var establishments = await _establishmentFile.ReadAllAsync();

        return establishments.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        var establishments = await _establishmentFile.ReadAllAsync();

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
        var allEstablishments = await _establishmentFile.ReadAllAsync();

        return allEstablishments.FirstOrDefault(x => x.URN == number || x.UKPRN == number || x.LAESTAB == number);
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        var establishmentEmails = await _establishmentEmailFile.ReadAllAsync();

        return establishmentEmails.FirstOrDefault(x => x.URN == urn);
    }
}
