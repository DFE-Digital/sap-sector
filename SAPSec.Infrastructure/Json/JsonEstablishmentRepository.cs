using Microsoft.Extensions.Logging;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;

namespace SAPSec.Infrastructure.Json
{
    public class JsonEstablishmentRepository : IEstablishmentRepository
    {
        private readonly IJsonFile<Establishment> _establishmentMetadataRepository;
        private ILogger<Establishment> _logger;

        public JsonEstablishmentRepository(
            IJsonFile<Establishment> establishmentMetadataRepository,
            ILogger<Establishment> logger)
        {
            _establishmentMetadataRepository = establishmentMetadataRepository;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
        {
            var establishments = await _establishmentMetadataRepository.ReadAllAsync();

            return establishments.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
        {
            var establishments = await _establishmentMetadataRepository.ReadAllAsync();

            return establishments.Where(e => urns.Contains(e.URN)).ToList().AsReadOnly();
        }


        public async Task<Establishment> GetEstablishmentAsync(string urn)
        {
            var allEstablishments = await GetEstablishmentsAsync([urn]);
            var establishment = allEstablishments.FirstOrDefault();

            return establishment ?? new Establishment();
        }

        public async Task<Establishment> GetEstablishmentByAnyNumberAsync(string number)
        {
            var allEstablishments = await _establishmentMetadataRepository.ReadAllAsync();

            return allEstablishments.FirstOrDefault(x => x.URN == number || x.UKPRN == number || x.DfENumberSearchable == number) ?? new Establishment();
        }
    }
}