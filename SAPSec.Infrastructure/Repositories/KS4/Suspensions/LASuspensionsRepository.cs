using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories.KS4.Suspensions
{
    public class LASuspensionsRepository : ILASuspensionsRepository
    {
        private readonly IGenericRepository<LASuspensions> _LASuspensionsRepository;
        private ILogger<LASuspensions> _logger;

        public LASuspensionsRepository(
            IGenericRepository<LASuspensions> LASuspensionsRepository,
            ILogger<LASuspensions> logger)
        {
            _LASuspensionsRepository = LASuspensionsRepository;
            _logger = logger;
        }


        public IEnumerable<LASuspensions> GetAllLASuspensions()
        {
            return _LASuspensionsRepository.ReadAll() ?? [];
        }


        public LASuspensions GetLASuspensions(string laCode)
        {
            return GetAllLASuspensions().FirstOrDefault(x => x.Id == laCode) ?? new LASuspensions();
        }
    }
}
