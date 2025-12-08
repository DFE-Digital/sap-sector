using SAPSec.Core.Model.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories.KS4.Absence
{
    public interface IEstablishmentAbsenceRepository
    {
        IEnumerable<EstablishmentAbsence> GetAllEstablishmentAbsence();
        EstablishmentAbsence GetEstablishmentAbsence(string urn);
    }
}
