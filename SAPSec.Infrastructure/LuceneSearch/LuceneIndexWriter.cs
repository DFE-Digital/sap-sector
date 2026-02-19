using Lucene.Net.Documents;
using Lucene.Net.Util;
using SAPSec.Core.Model;

namespace SAPSec.Infrastructure.LuceneSearch;

public class LuceneIndexWriter(LuceneIndexContext context)
{
    private static readonly FieldType TermVectorFieldType = new FieldType(TextField.TYPE_STORED)
    {
        StoreTermVectors = true,
        StoreTermVectorPositions = true,
        StoreTermVectorOffsets = true,
        StoreTermVectorPayloads = true,
        IsStored = true
    };

    public void BuildIndex(IEnumerable<Establishment> schools)
    {
        context.Writer.DeleteAll();

        foreach (var e in schools)
        {
            var doc = new Document
            {
                new StringField(FieldName.Urn, e.URN.ToString(), Field.Store.YES),
                new Field(FieldName.EstablishmentName, e.EstablishmentName, TermVectorFieldType),
                new SortedDocValuesField(FieldName.EstablishmentNameSort, new BytesRef(e.EstablishmentName)),
                new Field(FieldName.Street, e.Address3 ?? string.Empty, TermVectorFieldType),
                new Field(FieldName.Postcode, e.Postcode ?? string.Empty, TermVectorFieldType)
                // new Field(FieldName.EstablishmentNameSchoolId, e.EstablishmentNameSchoolId, TermVectorFieldType),
                // new Field(FieldName.EstablishmentNameSchoolIdAddress, e.EstablishmentNameSchoolIdAddress, TermVectorFieldType),
            };

            context.Writer.AddDocument(doc);
        }

        context.Writer.Flush(triggerMerge: true, applyAllDeletes: true);

        context.SearcherManager.MaybeRefreshBlocking();
    }
}
