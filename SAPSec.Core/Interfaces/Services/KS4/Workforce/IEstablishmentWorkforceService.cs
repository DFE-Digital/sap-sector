using SAPSec.Core.Model.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services.KS4.Workforce
{
    public interface IEstablishmentWorkforceService
    {
        IEnumerable<EstablishmentWorkforce> GetAllEstablishmentWorkforce();
        EstablishmentWorkforce GetEstablishmentWorkforce(string urn);
    }
}
