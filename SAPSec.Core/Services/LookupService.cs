using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;


        public LookupService(
            ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }


        public IEnumerable<Lookup> GetAllLookups()
        {
            return _lookupRepository.GetAllLookups();
        }


        public Lookup GetLookup(string urn)
        {
            return _lookupRepository.GetLookup(urn) ?? new();
        }
    }
}
