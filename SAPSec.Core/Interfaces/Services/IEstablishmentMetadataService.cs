using SAPSec.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Interfaces.Services
{
    public interface IEstablishmentMetadataService
    {
        IEnumerable<EstablishmentMetadata> GetAllEstablishmentMetadata();
        EstablishmentMetadata GetEstablishmentMetadata(string urn);
    }
}
