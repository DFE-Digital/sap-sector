using SAPSec.Core.Model.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories.KS4.Performance
{
    public interface IEstablishmentPerformanceRepository
    {
        IEnumerable<EstablishmentPerformance> GetAllEstablishmentPerformance();
        EstablishmentPerformance GetEstablishmentPerformance(string urn);
    }
}
