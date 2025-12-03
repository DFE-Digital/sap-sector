using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services.KS4.Suspensions
{
    public interface ILASuspensionsService
    {
        IEnumerable<LASuspensions> GetAllLASuspensions();
        LASuspensions GetLASuspensions(string id);
    }
}
