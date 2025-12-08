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
    public class EnglandSuspensionsService : IEnglandSuspensionsService
    {
        private readonly IEnglandSuspensionsRepository _englandSuspensionsRepository;

        public EnglandSuspensionsService(IEnglandSuspensionsRepository englandSuspensionsRepository)
        {
            _englandSuspensionsRepository = englandSuspensionsRepository;
        }

        public EnglandSuspensions GetEnglandSuspensions()
        {
            return _englandSuspensionsRepository.GetEnglandSuspensions();
        }
    }
}
