using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Suspensions
{
    public class EstablishmentSuspensionsRepository : IEstablishmentSuspensionsRepository
    {
        private readonly IGenericRepository<EstablishmentSuspensions> _establishmentSuspensionsRepository;
        private ILogger<EstablishmentSuspensions> _logger;

        public EstablishmentSuspensionsRepository(
            IGenericRepository<EstablishmentSuspensions> establishmentSuspensionsRepository,
            ILogger<EstablishmentSuspensions> logger)
        {
            _establishmentSuspensionsRepository = establishmentSuspensionsRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentSuspensions> GetAllEstablishmentSuspensions()
        {
            return _establishmentSuspensionsRepository.ReadAll() ?? [];
        }


        public EstablishmentSuspensions GetEstablishmentSuspensions(string urn)
        {
            return GetAllEstablishmentSuspensions().FirstOrDefault(x => x.Id == urn) ?? new EstablishmentSuspensions();
        }
    }
}
