using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Model.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Absence
{
    public class LAAbsenceService : ILAAbsenceService
    {
        private readonly ILAAbsenceRepository _LAAbsenceRepository;

        public LAAbsenceService(ILAAbsenceRepository LAAbsenceRepository)
        {
            _LAAbsenceRepository = LAAbsenceRepository;
        }


        public IEnumerable<LAAbsence> GetAllLAAbsence()
        {
            return _LAAbsenceRepository.GetAllLAAbsence();
        }


        public LAAbsence GetLAAbsence(string la)
        {
            return _LAAbsenceRepository.GetLAAbsence(la);
        }
    }
}
