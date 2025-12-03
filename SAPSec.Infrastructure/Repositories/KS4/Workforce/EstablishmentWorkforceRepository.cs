using Microsoft.Extensions.Logging;
using SAPSec.Core.Entities;
using SAPSec.Core.Entities.KS4.Workforce;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Workforce
{
    public class EstablishmentWorkforceRepository : IEstablishmentWorkforceRepository
    {

        private readonly IGenericRepository<EstablishmentWorkforce> _establishmentWorkforceRepository;
        private ILogger<EstablishmentWorkforce> _logger;

        public EstablishmentWorkforceRepository(
            IGenericRepository<EstablishmentWorkforce> establishmentWorkforceRepository, 
            ILogger<EstablishmentWorkforce> logger)
        {
            _establishmentWorkforceRepository = establishmentWorkforceRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentWorkforce> GetAllEstablishmentWorkforce()
        {
            return _establishmentWorkforceRepository.ReadAll() ?? [];
        }


        public EstablishmentWorkforce GetEstablishmentWorkforce(string urn)
        {
            return GetAllEstablishmentWorkforce().First(x => x.Id == urn) ?? new EstablishmentWorkforce();
        }
    }
}
