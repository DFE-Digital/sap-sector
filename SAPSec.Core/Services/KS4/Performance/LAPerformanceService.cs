using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Interfaces.Services.KS4.Performance;
using SAPSec.Core.Model.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Performance
{
    public class LAPerformanceService : ILAPerformanceService
    {
        private readonly ILAPerformanceRepository _LAPerformanceRepository;

        public LAPerformanceService(ILAPerformanceRepository LAPerformanceRepository)
        {
            _LAPerformanceRepository = LAPerformanceRepository;
        }


        public IEnumerable<LAPerformance> GetAllLAPerformance()
        {
            return _LAPerformanceRepository.GetAllLAPerformance();
        }


        public LAPerformance GetLAPerformance(string urn)
        {
            return _LAPerformanceRepository.GetLAPerformance(urn);
        }
    }
}
