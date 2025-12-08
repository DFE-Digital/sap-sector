using SAPSec.Core.Model;

namespace SAPSec.Infrastructure.LuceneSearch.Interfaces;

public interface ILuceneIndexWriter
{
    void BuildIndex(IEnumerable<Establishment> schools);
}