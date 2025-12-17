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
    public class LASubjectEntriesRepository : ILASubjectEntriesRepository
    {
        private readonly IGenericRepository<LASubjectEntries> _LASubjectEntriesRepository;
        private ILogger<LASubjectEntries> _logger;

        public LASubjectEntriesRepository(
            IGenericRepository<LASubjectEntries> LASubjectEntriesRepository,
            ILogger<LASubjectEntries> logger)
        {
            _LASubjectEntriesRepository = LASubjectEntriesRepository;
            _logger = logger;
        }


        public IEnumerable<LASubjectEntries> GetAllLASubjectEntries()
        {
            return _LASubjectEntriesRepository.ReadAll() ?? [];
        }


        public LASubjectEntries GetLASubjectEntries(string laCode)
        {
            return GetAllLASubjectEntries().FirstOrDefault(x => x.Id == laCode) ?? new LASubjectEntries();
        }
    }
}
