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
    public class EstablishmentSubjectEntriesRepository : IEstablishmentSubjectEntriesRepository
    {
        private readonly IGenericRepository<EstablishmentSubjectEntries> _establishmentSubjectEntriesRepository;
        private ILogger<EstablishmentSubjectEntries> _logger;

        public EstablishmentSubjectEntriesRepository(
            IGenericRepository<EstablishmentSubjectEntries> establishmentSubjectEntriesRepository,
            ILogger<EstablishmentSubjectEntries> logger)
        {
            _establishmentSubjectEntriesRepository = establishmentSubjectEntriesRepository;
            _logger = logger;
        }


        public IEnumerable<EstablishmentSubjectEntries> GetAllEstablishmentSubjectEntries()
        {
            return _establishmentSubjectEntriesRepository.ReadAll() ?? [];
        }


        public EstablishmentSubjectEntries GetEstablishmentSubjectEntries(string urn)
        {
            return GetAllEstablishmentSubjectEntries().FirstOrDefault(x => x.Id == urn) ?? new EstablishmentSubjectEntries();
        }
    }
}
