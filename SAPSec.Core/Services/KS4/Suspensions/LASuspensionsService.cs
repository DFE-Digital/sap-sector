using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Interfaces.Services.KS4.Suspensions;
using SAPSec.Core.Model.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services.KS4.Suspensions
{
    public class LASuspensionsService : ILASuspensionsService
    {
        private readonly ILASuspensionsRepository _LASuspensionsRepository;

        public LASuspensionsService(ILASuspensionsRepository LASuspensionsRepository)
        {
            _LASuspensionsRepository = LASuspensionsRepository;
        }


        public IEnumerable<LASuspensions> GetAllLASuspensions()
        {
            return _LASuspensionsRepository.GetAllLASuspensions();
        }


        public LASuspensions GetLASuspensions(string urn)
        {
            return _LASuspensionsRepository.GetLASuspensions(urn);
        }
    }
}
