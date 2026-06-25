using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Infrastructure.Json;

public class JsonEstablishmentEmailStore : IEstablishmentEmailStore
{
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailFile;

    private ILogger<JsonEstablishmentEmailStore> _logger;

    public JsonEstablishmentEmailStore(
        IJsonFile<EstablishmentEmail> establishmentEmailFile,
        ILogger<JsonEstablishmentEmailStore> logger)
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
