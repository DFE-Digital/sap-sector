using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Model.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Performance
{
    public class EstablishmentPerformanceRepository : IEstablishmentPerformanceRepository
    {
        private readonly IGenericRepository<EstablishmentPerformance> _establishmentPerformanceRepository;
        private ILogger<EstablishmentPerformance> _logger;

        public EstablishmentPerformanceRepository(
            IGenericRepository<EstablishmentPerformance> establishmentPerformanceRepository,
            ILogger<EstablishmentPerformance> logger)
        {
            _establishmentPerformanceRepository = establishmentPerformanceRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentPerformance> GetAllEstablishmentPerformance()
        {
            return _establishmentPerformanceRepository.ReadAll() ?? [];
        }


        public EstablishmentPerformance GetEstablishmentPerformance(string urn)
        {
            return GetAllEstablishmentPerformance().First(x => x.Id == urn) ?? new EstablishmentPerformance();
        }
    }
}
