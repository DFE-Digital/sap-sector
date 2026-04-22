using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonEstablishmentEmailRepository : IEstablishmentEmailRepository
{
    private readonly IJsonFile<EstablishmentEmail> _establishmentEmailJsonFile;

    private ILogger<JsonEstablishmentEmailRepository> _logger;

    public JsonEstablishmentEmailRepository(
        IJsonFile<EstablishmentEmail> establishmentEmailJsonFile,
        ILogger<JsonEstablishmentEmailRepository> logger)
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
