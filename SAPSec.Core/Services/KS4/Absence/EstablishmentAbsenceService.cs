using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Model.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPPub.Core.Services.KS4.Absence
{
    public class EstablishmentAbsenceService : IEstablishmentAbsenceService
    {
        private readonly IEstablishmentAbsenceRepository _establishmentAbsenceRepository;


        public EstablishmentAbsenceService(
            IEstablishmentAbsenceRepository establishmentAbsenceRepository)
        {
            _establishmentAbsenceRepository = establishmentAbsenceRepository;
        }


        public IEnumerable<EstablishmentAbsence> GetAllEstablishmentAbsence()
        {
            return _establishmentAbsenceRepository.GetAllEstablishmentAbsence();
        }


        public EstablishmentAbsence GetEstablishmentAbsence(string urn)
        {
            return _establishmentAbsenceRepository.GetEstablishmentAbsence(urn);
        }
    }
}
