using nietras.SeparatedValues;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Infrastructure.Repositories;

public class SchoolCsvFileRepository(string csvPath) : ISchoolRepository
{
    private IList<School>? _schools;

    public IList<School> GetAll()
    {
        _schools ??= ParseEstablishments();

        return _schools;
    }

    public School GetSchoolByUrn(int schoolNumber)
    {
        _schools ??= ParseEstablishments();

        return _schools.Single(sch => sch.Urn == schoolNumber);
    }

    public School? GetSchoolByNumber(int schoolNumber)
    {
        _schools ??= ParseEstablishments();

        return _schools.FirstOrDefault(sch => sch.Urn == schoolNumber || sch.Ukprn == schoolNumber || sch.SearchAbleDfENumber == schoolNumber);
    }

    private List<School> ParseEstablishments()
    {
        if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath)) return [];

        var sepReader = Sep.Reader(o => o with { HasHeader = true, Unescape = true, DisableColCountCheck = true }).FromFile(csvPath);

        return sepReader.Enumerate((SepReader.Row row, out School school) =>
        {
            school = null!;

            try
            {
                if (row.ColCount < 5) return false;

                row[FieldName.Urn].TryParse<int>(out var urn);
                row[FieldName.Ukprn].TryParse<int>(out var ukprn);
                row[FieldName.LaCode].TryParse<int>(out var laCode);
                row[FieldName.EstablishmentNumber].TryParse<int>(out var establishmentNumber);
                var schoolName = row[FieldName.EstablishmentName].ToString().Trim();

                school = new School(urn, ukprn, laCode, establishmentNumber, schoolName);

                // var street = row[FieldName.Street].ToString().Trim();
                // var locality = row[FieldName.Locality].ToString().Trim();
                // var address3 = row[FieldName.Address3].ToString().Trim();
                // var town = row[FieldName.Town].ToString().Trim();
                // var county = row[FieldName.County].ToString().Trim();
                // var postcode = row[FieldName.Postcode].ToString().Trim();

                //school = new School(urn, ukprn, laCode, establishmentNumber, schoolName, street, locality, address3, town, county, postcode);

                return true;
            }
            catch
            {
                //at this stage I don't care if any of these fields are missing or non-numeric or any other error
                return false;
            }
        }).ToList();
    }
}