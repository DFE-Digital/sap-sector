using SAPSec.Core.Model.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories.KS4.Absence
{
    public interface ILAAbsenceRepository
    {
        IEnumerable<LAAbsence> GetAllLAAbsence();
        LAAbsence GetLAAbsence(string id);
    }
}
