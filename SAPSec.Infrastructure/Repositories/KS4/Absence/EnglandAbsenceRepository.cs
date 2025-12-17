using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Model.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Absence
{
    public class EnglandAbsenceRepository: IEnglandAbsenceRepository
    {
        private readonly IGenericRepository<EnglandAbsence> _establishmentAbsenceRepository;
        private ILogger<EnglandAbsence> _logger;

        public EnglandAbsenceRepository(
            IGenericRepository<EnglandAbsence> establishmentAbsenceRepository,
            ILogger<EnglandAbsence> logger)
        {
            _establishmentAbsenceRepository = establishmentAbsenceRepository;
            _logger = logger;
        }


        public EnglandAbsence GetEnglandAbsence()
        {
            return _establishmentAbsenceRepository.ReadAll()?.FirstOrDefault() ?? new EnglandAbsence();
        }
    }
}
