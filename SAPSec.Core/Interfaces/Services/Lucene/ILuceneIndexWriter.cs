using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Interfaces.Services.Lucene;

public interface ILuceneIndexWriter
{
    void BuildIndex(IList<School> schools);
}