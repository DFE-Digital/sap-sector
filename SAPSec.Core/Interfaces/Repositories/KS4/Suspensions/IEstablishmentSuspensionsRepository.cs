using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories.KS4.Suspensions
{
    public interface IEstablishmentSuspensionsRepository
    {
        IEnumerable<EstablishmentSuspensions> GetAllEstablishmentSuspensions();
        EstablishmentSuspensions GetEstablishmentSuspensions(string urn);
    }
}
