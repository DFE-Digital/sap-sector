using SAPSec.Core.Interfaces.Repositories.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Services.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.SubjectEntries
{
    public class EnglandSubjectEntriesService : IEnglandSubjectEntriesService
    {
        private readonly IEnglandSubjectEntriesRepository _englandSubjectEntriesRepository;

        public EnglandSubjectEntriesService(IEnglandSubjectEntriesRepository englandSubjectEntriesRepository)
        {
            _englandSubjectEntriesRepository = englandSubjectEntriesRepository;
        }

        public EnglandSubjectEntries GetEnglandSubjectEntries()
        {
            return _englandSubjectEntriesRepository.GetEnglandSubjectEntries();
        }
    }
}
