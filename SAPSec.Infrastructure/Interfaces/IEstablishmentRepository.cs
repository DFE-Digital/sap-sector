using SAPSec.Infrastructure.Entities;

namespace SAPSec.Infrastructure.Interfaces;

public interface IEstablishmentRepository
{
    IEnumerable<Establishment> GetAll();
}
