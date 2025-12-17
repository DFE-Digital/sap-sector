using SAPSec.Core.Model.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Repositories.KS4.SubjectEntries
{
    public interface IEstablishmentSubjectEntriesRepository
    {
        IEnumerable<EstablishmentSubjectEntries> GetAllEstablishmentSubjectEntries();
        EstablishmentSubjectEntries GetEstablishmentSubjectEntries(string urn);
    }
}
