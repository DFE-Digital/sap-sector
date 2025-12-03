using Microsoft.Extensions.Logging;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories
{
    public class EstablishmentRepository : IEstablishmentRepository
    {
        private readonly IGenericRepository<Establishment> _establishmentMetadataRepository;
        private ILogger<Establishment> _logger;

        public EstablishmentRepository(
            IGenericRepository<Establishment> establishmentMetadataRepository, 
            ILogger<Establishment> logger)
        {
            _establishmentMetadataRepository = establishmentMetadataRepository;
            _logger = logger;
        }


        public IEnumerable<Establishment> GetAllEstablishments()
        {
            return _establishmentMetadataRepository.ReadAll() ?? [];
        }


        public Establishment GetEstablishment(string urn)
        {
            return GetAllEstablishments().First(x => x.URN == urn) ?? new Establishment();
        }
    }
}
