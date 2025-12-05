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
    public class EstablishmentService : IEstablishmentService
    {
        private readonly IEstablishmentRepository _establishmentRepository;
        private ILogger<Establishment> _logger;


        public EstablishmentService(
            IEstablishmentRepository establishmentRepository,
            ILogger<Establishment> logger)
        {
            _establishmentRepository = establishmentRepository;
            _logger = logger;
        }


        public IEnumerable<Establishment> GetAllEstablishments()
        {
            return _establishmentRepository.GetAllEstablishments();
        }


        public Establishment GetEstablishment(string urn)
        {
            return _establishmentRepository.GetEstablishment(urn);
        }

        public Establishment GetEstablishmentByAnyNumber(string number)
        {
            return _establishmentRepository.GetEstablishmentByAnyNumber(number);
        }
    }
}
