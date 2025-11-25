using Lucene.Net.Documents;
using Lucene.Net.Util;
using SAPSec.Core.Interfaces.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Services.Lucene;

public class LuceneIndexWriter(LuceneIndexContext context) : ILuceneIndexWriter
{
    private static readonly FieldType TermVectorFieldType = new FieldType(TextField.TYPE_STORED)
    {
        StoreTermVectors = true,
        StoreTermVectorPositions = true,
        StoreTermVectorOffsets = true,
        StoreTermVectorPayloads = true,
        IsStored = true
    };

    public void BuildIndex(IList<School> schools)
    {
        context.Writer.DeleteAll();

        foreach (var e in schools)
        {
            var doc = new Document
            {
                new StringField(FieldName.Urn, e.Urn.ToString(), Field.Store.YES),
                new Field(FieldName.EstablishmentName, e.EstablishmentName, TermVectorFieldType),
                new SortedDocValuesField(FieldName.EstablishmentNameSort, new BytesRef(e.EstablishmentName))
                // new Field(FieldName.EstablishmentNameSchoolId, e.EstablishmentNameSchoolId, TermVectorFieldType),
                // new Field(FieldName.EstablishmentNameSchoolIdAddress, e.EstablishmentNameSchoolIdAddress, TermVectorFieldType),
            };

            context.Writer.AddDocument(doc);
        }

        context.Writer.Flush(triggerMerge: true, applyAllDeletes: true);

        context.SearcherManager.MaybeRefreshBlocking();
    }
}