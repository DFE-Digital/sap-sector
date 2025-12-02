using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Integration.Tests.Mocks;

public class MockSchoolRepository : ISchoolRepository
{
    private readonly List<School> _schools = new()
        {
            new School(102848, 123456, 204, 3658, "Test School A"),
            new School(100273, 654321, 204, 3658, "Saint Paul Roman Catholic"),
            new School(999999, 111111, 200, 1234, "Mock Academy Trust School")
        };

    public School? GetSchoolByUrn(int urn)
        => _schools.FirstOrDefault(s => s.Urn == urn);

    public School? GetSchoolByNumber(int number)
        => _schools.FirstOrDefault(s =>
            s.Urn == number ||
            s.Ukprn == number ||
        s.SearchAbleDfENumber == number
        );

    public IList<School> GetAll()
        => _schools;
}
