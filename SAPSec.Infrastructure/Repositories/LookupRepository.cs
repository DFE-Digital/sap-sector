using Microsoft.Extensions.Logging;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IGenericRepository<Lookup> _lookupMetadataRepository;
        private ILogger<Lookup> _logger;

        public LookupRepository(
            IGenericRepository<Lookup> lookupMetadataRepository, 
            ILogger<Lookup> logger)
        {
            _lookupMetadataRepository = lookupMetadataRepository;
            _logger = logger;
        }


        public IEnumerable<Lookup> GetAllLookups()
        {
            return _lookupMetadataRepository.ReadAll() ?? [];
        }


        public Lookup GetLookup(string id)
        {
            return GetAllLookups().FirstOrDefault(x => x.Id == id) ?? new Lookup();
        }
    }
}
