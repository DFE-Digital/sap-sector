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
    public class LADestinationsRepository : ILADestinationsRepository
    {
        private readonly IGenericRepository<LADestinations> _LADestinationsRepository;
        private ILogger<LADestinations> _logger;

        public LADestinationsRepository(
            IGenericRepository<LADestinations> LADestinationsRepository,
            ILogger<LADestinations> logger)
        {
            _LADestinationsRepository = LADestinationsRepository;
            _logger = logger;
        }


        public IEnumerable<LADestinations> GetAllLADestinations()
        {
            return _LADestinationsRepository.ReadAll() ?? [];
        }


        public LADestinations GetLADestinations(string laCode)
        {
            return GetAllLADestinations().First(x => x.Id == laCode) ?? new LADestinations();
        }
    }
}
