using SAPSec.Core.Model.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services.KS4.Destinations
{
    public interface IEstablishmentDestinationsService
    {
        IEnumerable<EstablishmentDestinations> GetAllEstablishmentDestinations();
        EstablishmentDestinations GetEstablishmentDestinations(string urn);
    }
}
