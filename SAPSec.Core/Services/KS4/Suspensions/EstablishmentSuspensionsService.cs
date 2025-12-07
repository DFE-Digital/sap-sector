using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Interfaces.Services.KS4.Suspensions;
using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Suspensions
{
    public class EstablishmentSuspensionsService : IEstablishmentSuspensionsService
    {
        private readonly IEstablishmentSuspensionsRepository _establishmentSuspensionsRepository;

        public EstablishmentSuspensionsService(IEstablishmentSuspensionsRepository establishmentSuspensionsRepository)
        {
            _establishmentSuspensionsRepository = establishmentSuspensionsRepository;
        }


        public IEnumerable<EstablishmentSuspensions> GetAllEstablishmentSuspensions()
        {
            return _establishmentSuspensionsRepository.GetAllEstablishmentSuspensions();
        }


        public EstablishmentSuspensions GetEstablishmentSuspensions(string urn)
        {
            return _establishmentSuspensionsRepository.GetEstablishmentSuspensions(urn);
        }
    }
}
