using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Model.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Destinations
{
    public class EnglandDestinationsRepository : IEnglandDestinationsRepository
    {
        private readonly IGenericRepository<EnglandDestinations> _EnglandDestinationsRepository;
        private ILogger<EnglandDestinations> _logger;

        public EnglandDestinationsRepository(
            IGenericRepository<EnglandDestinations> EnglandDestinationsRepository,
            ILogger<EnglandDestinations> logger)
        {
            _EnglandDestinationsRepository = EnglandDestinationsRepository;
            _logger = logger;
        }


        public EnglandDestinations GetEnglandDestinations()
        {
            return _EnglandDestinationsRepository.ReadAll()?.FirstOrDefault() ?? new EnglandDestinations();
        }
    }
}
