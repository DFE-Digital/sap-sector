using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories
{
    public interface IEstablishmentRepository
    {
        IEnumerable<Establishment> GetAllEstablishments();
        Establishment GetEstablishment(string urn);
        Establishment GetEstablishmentByAnyNumber(string number);
    }
}
