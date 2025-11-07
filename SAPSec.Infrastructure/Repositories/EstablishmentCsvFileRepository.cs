using nietras.SeparatedValues;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Infrastructure.Repositories;

public class EstablishmentCsvFileRepository(string csvPath) : IEstablishmentRepository
{
    //TODO What do we do if the file doesn't exist?'
    private readonly string _csvPath = csvPath /*?? throw new ArgumentNullException(nameof(csvPath))*/ ;

    public IEnumerable<Establishment> GetAll()
    {
        //TODO What do we do if the file doesn't exist?'
        if (string.IsNullOrWhiteSpace(_csvPath) || !File.Exists(_csvPath)) yield break;

        var list = ParseEstablishments();
        foreach (var e in list)
        {
            yield return e;
        }
    }

    private List<Establishment> ParseEstablishments()
    {
        var results = new List<Establishment>();
        var sepReader = Sep.Reader(o => o with { HasHeader = true }).FromFile(_csvPath);
        foreach (var row in sepReader)
        {
            int id;
            if (row["EstablishmentNumber"].TryParse(out id))
            {
                var name = row["EstablishmentName"].ToString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    results.Add(new Establishment(id, name.Trim()));
                }
            }
        }
        return results;
    }
}
