using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Data.Json;

public class JsonEstablishmentEmailStore : IEstablishmentEmailStore
{
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailJsonFile;

    private ILogger<JsonEstablishmentEmailStore> _logger;

    public JsonEstablishmentEmailStore(
        IJsonFile<EstablishmentEmail> establishmentEmailJsonFile,
        ILogger<JsonEstablishmentEmailStore> logger)
    {
        _establishmentEmailJsonFile = establishmentEmailJsonFile;
        _logger = logger;
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        var establishmentEmails = await _establishmentEmailJsonFile.ReadAllAsync();

        return establishmentEmails.FirstOrDefault(x => x.URN == urn);
    }
}
