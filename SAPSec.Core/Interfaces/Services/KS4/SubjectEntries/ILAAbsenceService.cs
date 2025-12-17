using SAPSec.Core.Model.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services.KS4.SubjectEntries
{
    public interface ILASubjectEntriesService
    {
        IEnumerable<LASubjectEntries> GetAllLASubjectEntries();
        LASubjectEntries GetLASubjectEntries(string id);
    }
}
