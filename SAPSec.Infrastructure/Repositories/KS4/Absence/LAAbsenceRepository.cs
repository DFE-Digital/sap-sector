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
    public class LAAbsenceRepository: ILAAbsenceRepository
    {
        private readonly IGenericRepository<LAAbsence> _establishmentAbsenceRepository;
        private ILogger<LAAbsence> _logger;

        public LAAbsenceRepository(
            IGenericRepository<LAAbsence> establishmentAbsenceRepository,
            ILogger<LAAbsence> logger)
        {
            _establishmentAbsenceRepository = establishmentAbsenceRepository;
            _logger = logger;
        }


        public IEnumerable<LAAbsence> GetAllLAAbsence()
        {
            return _establishmentAbsenceRepository.ReadAll() ?? [];
        }


        public LAAbsence GetLAAbsence(string laCode)
        {
            return GetAllLAAbsence().FirstOrDefault(x => x.Id == laCode) ?? new LAAbsence();
        }
    }
}
