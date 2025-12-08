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
    public class EnglandAbsenceService : IEnglandAbsenceService
    {
        private readonly IEnglandAbsenceRepository _EnglandAbsenceRepository;

        public EnglandAbsenceService(IEnglandAbsenceRepository EnglandAbsenceRepository)
        {
            _EnglandAbsenceRepository = EnglandAbsenceRepository;
        }


        public EnglandAbsence GetEnglandAbsence()
        {
            return _EnglandAbsenceRepository.GetEnglandAbsence();
        }
    }
}
