using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Interfaces.Services.KS4.Performance;
using SAPSec.Core.Model.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPPub.Core.Services.KS4.Performance
{
    public class EstablishmentPerformanceService : IEstablishmentPerformanceService
    {
        private readonly IEstablishmentPerformanceRepository _establishmentPerformanceRepository;

        public EstablishmentPerformanceService(IEstablishmentPerformanceRepository establishmentPerformanceRepository)
        {
            _establishmentPerformanceRepository = establishmentPerformanceRepository;
        }


        public IEnumerable<EstablishmentPerformance> GetAllEstablishmentPerformance()
        {
            return _establishmentPerformanceRepository.GetAllEstablishmentPerformance();
        }


        public EstablishmentPerformance GetEstablishmentPerformance(string urn)
        {
            return _establishmentPerformanceRepository.GetEstablishmentPerformance(urn);
        }
    }
}
