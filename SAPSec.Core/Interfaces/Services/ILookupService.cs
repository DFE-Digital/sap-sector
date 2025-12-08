using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services
{
    public interface ILookupService
    {
        IEnumerable<Lookup> GetAllLookups();
        Lookup GetLookup(string urn);
    }
}
