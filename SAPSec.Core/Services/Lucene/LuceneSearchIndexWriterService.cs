using Lucene.Net.Documents;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Core.Services.Lucene;

public class LuceneSearchIndexWriterService(IEstablishmentRepository repository, LuceneIndexContext context)
{
    private const string FieldId = "id";
    private const string FieldName = "name";

    public void BuildIndex()
    {
        context.Writer.DeleteAll();

        foreach (var e in repository.GetAll())
        {
            var doc = new Document
            {
                new StringField(FieldId, e.EstablishmentNumber.ToString(), Field.Store.YES),
                new TextField(FieldName, e.EstablishmentName, Field.Store.YES),
            };
            context.Writer.AddDocument(doc);
        }
        context.Writer.Flush(triggerMerge: true, applyAllDeletes: true);
        context.SearcherManager.MaybeRefreshBlocking();
    }
}