using Microsoft.Extensions.Logging;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;

namespace SAPSec.Infrastructure.Repositories.Json
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IJsonFile<Lookup> _lookupMetadataRepository;
        private ILogger<Lookup> _logger;

        public LookupRepository(
            IJsonFile<Lookup> lookupMetadataRepository,
            ILogger<Lookup> logger)
        {
            _lookupMetadataRepository = lookupMetadataRepository;
            _logger = logger;
        }


        public async Task<IEnumerable<Lookup>> GetAllLookupsAsync()
        {
            return (await _lookupMetadataRepository.ReadAllAsync()) ?? [];
        }


        public async Task<Lookup?> GetLookupAsync(string id)
        {
            return (await GetAllLookupsAsync()).FirstOrDefault(x => x.Id == id) ?? new Lookup();
        }
    }
}
