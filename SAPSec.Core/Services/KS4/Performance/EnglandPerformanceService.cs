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
    public class EnglandPerformanceService : IEnglandPerformanceService
    {
        private readonly IEnglandPerformanceRepository _englandPerformanceRepository;

        public EnglandPerformanceService(IEnglandPerformanceRepository englandPerformanceRepository)
        {
            _englandPerformanceRepository = englandPerformanceRepository;
        }

        public EnglandPerformance GetEnglandPerformance()
        {
            return _englandPerformanceRepository.GetEnglandPerformance();
        }
    }
}
