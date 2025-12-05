using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Interfaces.Services.KS4.Destinations;
using SAPSec.Core.Model.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPPub.Core.Services.KS4.Destinations
{
    public class LADestinationsService : ILADestinationsService
    {
        private readonly ILADestinationsRepository _LADestinationsRepository;

        public LADestinationsService(ILADestinationsRepository LADestinationsRepository)
        {
            _LADestinationsRepository = LADestinationsRepository;
        }


        public IEnumerable<LADestinations> GetAllLADestinations()
        {
            return _LADestinationsRepository.GetAllLADestinations();
        }


        public LADestinations GetLADestinations(string la)
        {
            return _LADestinationsRepository.GetLADestinations(la);
        }
    }
}
