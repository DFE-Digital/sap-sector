using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonEstablishmentEmailRepository : IEstablishmentEmailRepository
{
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailFile;

    private ILogger<JsonEstablishmentEmailRepository> _logger;

    public JsonEstablishmentEmailRepository(
        IJsonFile<EstablishmentEmail> establishmentEmailFile,
        ILogger<JsonEstablishmentEmailRepository> logger)
    {
        _establishmentEmailFile = establishmentEmailFile;
        _logger = logger;
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        var establishmentEmails = await _establishmentEmailFile.ReadAllAsync();

        return establishmentEmails.FirstOrDefault(x => x.URN == urn);
    }
}
