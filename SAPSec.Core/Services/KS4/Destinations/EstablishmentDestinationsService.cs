using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Interfaces.Services.KS4.Destinations;
using SAPSec.Core.Model.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Destinations
{
    public class EstablishmentDestinationsService : IEstablishmentDestinationsService
    {
        private readonly IEstablishmentDestinationsRepository _establishmentDestinationsRepository;

        public EstablishmentDestinationsService(IEstablishmentDestinationsRepository establishmentDestinationsRepository)
        {
            _establishmentDestinationsRepository = establishmentDestinationsRepository;
        }


        public IEnumerable<EstablishmentDestinations> GetAllEstablishmentDestinations()
        {
            return _establishmentDestinationsRepository.GetAllEstablishmentDestinations();
        }


        public EstablishmentDestinations GetEstablishmentDestinations(string urn)
        {
            return _establishmentDestinationsRepository.GetEstablishmentDestinations(urn);
        }
    }
}
