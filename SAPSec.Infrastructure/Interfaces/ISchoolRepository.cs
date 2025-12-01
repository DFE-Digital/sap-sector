using SAPSec.Infrastructure.Entities;

namespace SAPSec.Infrastructure.Interfaces;

public interface ISchoolRepository
{
    IList<School> GetAll();
    School GetSchoolByUrn(int schoolNumber);
    School? GetSchoolByNumber(int schoolNumber);
}
