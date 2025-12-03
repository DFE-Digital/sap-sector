using Microsoft.Extensions.Logging;
using SAPSec.Core.Entities.KS4.Absence;
using SAPSec.Core.Entities.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Performance
{
    public class LAPerformanceRepository : ILAPerformanceRepository
    {
        private readonly IGenericRepository<LAPerformance> _LAPerformanceRepository;
        private ILogger<LAPerformance> _logger;

        public LAPerformanceRepository(
            IGenericRepository<LAPerformance> LAPerformanceRepository,
            ILogger<LAPerformance> logger)
        {
            _LAPerformanceRepository = LAPerformanceRepository;
            _logger = logger;
        }


        public IEnumerable<LAPerformance> GetAllLAPerformance()
        {
            return _LAPerformanceRepository.ReadAll() ?? [];
        }


        public LAPerformance GetLAPerformance(string laCode)
        {
            return GetAllLAPerformance().First(x => x.Id == laCode) ?? new LAPerformance();
        }
    }
}
