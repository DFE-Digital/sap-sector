using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.SubjectEntries
{
    public class EnglandSubjectEntriesRepository : IEnglandSubjectEntriesRepository
    {
        private readonly IGenericRepository<EnglandSubjectEntries> _EnglandSubjectEntriesRepository;
        private ILogger<EnglandSubjectEntries> _logger;

        public EnglandSubjectEntriesRepository(
            IGenericRepository<EnglandSubjectEntries> EnglandSubjectEntriesRepository,
            ILogger<EnglandSubjectEntries> logger)
        {
            _EnglandSubjectEntriesRepository = EnglandSubjectEntriesRepository;
            _logger = logger;
        }

        public EnglandSubjectEntries GetEnglandSubjectEntries()
        {
            return _EnglandSubjectEntriesRepository.ReadAll()?.FirstOrDefault() ?? new EnglandSubjectEntries();
        }
    }
}
