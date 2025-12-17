using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using SAPSec.Core.Interfaces.Services.KS4.Workforce;
using SAPSec.Core.Model.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Workforce
{
    public class EstablishmentWorkforceService : IEstablishmentWorkforceService
    {
        private readonly IEstablishmentWorkforceRepository _establishmentWorkforceRepository;


        public EstablishmentWorkforceService(
            IEstablishmentWorkforceRepository establishmentWorkforceRepository)
        {
            _establishmentWorkforceRepository = establishmentWorkforceRepository;
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
