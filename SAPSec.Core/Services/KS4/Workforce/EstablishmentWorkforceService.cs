using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using SAPSec.Core.Interfaces.Services.KS4.Workforce;
using SAPSec.Core.Model.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPPub.Core.Services.KS4.Workforce
{
    public class EstablishmentWorkforceService : IEstablishmentWorkforceService
    {
        private readonly IEstablishmentWorkforceRepository _establishmentWorkforceRepository;
        private ILogger<EstablishmentWorkforce> _logger;


        public EstablishmentWorkforceService(
            IEstablishmentWorkforceRepository establishmentWorkforceRepository,
            ILogger<EstablishmentWorkforce> logger)
        {
            _establishmentWorkforceRepository = establishmentWorkforceRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentWorkforce> GetAllEstablishmentWorkforce()
        {
            return _establishmentWorkforceRepository.GetAllEstablishmentWorkforce();
        }


        public EstablishmentWorkforce GetEstablishmentWorkforce(string urn)
        {
            return _establishmentWorkforceRepository.GetEstablishmentWorkforce(urn);
        }
    }
}
