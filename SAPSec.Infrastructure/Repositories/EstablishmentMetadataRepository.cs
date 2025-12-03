using Microsoft.Extensions.Logging;
using SAPSec.Core.Entities;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories
{
    public class EstablishmentMetadataRepository : IEstablishmentMetadataRepository
    {
        private readonly IGenericRepository<EstablishmentMetadata> _establishmentMetadataRepository;
        private ILogger<EstablishmentMetadata> _logger;

        public EstablishmentMetadataRepository(
            IGenericRepository<EstablishmentMetadata> establishmentMetadataRepository, 
            ILogger<EstablishmentMetadata> logger)
        {
            _establishmentMetadataRepository = establishmentMetadataRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentMetadata> GetAllEstablishmentMetadata()
        {
            return _establishmentMetadataRepository.ReadAll() ?? [];
        }


        public EstablishmentMetadata GetEstablishmentMetadata(string id)
        {
            return GetAllEstablishmentMetadata().First(x => x.Id == id) ?? new EstablishmentMetadata();
        }
    }
}
