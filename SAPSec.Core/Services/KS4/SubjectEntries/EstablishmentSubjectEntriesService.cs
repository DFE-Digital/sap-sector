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
    public class EstablishmentSubjectEntriesService : IEstablishmentSubjectEntriesService
    {
        private readonly IEstablishmentSubjectEntriesRepository _establishmentSubjectEntriesRepository;

        public EstablishmentSubjectEntriesService(IEstablishmentSubjectEntriesRepository establishmentSubjectEntriesRepository)
        {
            _establishmentSubjectEntriesRepository = establishmentSubjectEntriesRepository;
        }


        public IEnumerable<EstablishmentSubjectEntries> GetAllEstablishmentSubjectEntries()
        {
            return _establishmentSubjectEntriesRepository.GetAllEstablishmentSubjectEntries();
        }


        public EstablishmentSubjectEntries GetEstablishmentSubjectEntries(string urn)
        {
            return _establishmentSubjectEntriesRepository.GetEstablishmentSubjectEntries(urn);
        }
    }
}
