using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>A catalogue definition that expands to DataMap rows.</summary>
public interface IDataMapDefinition
{
    IReadOnlyList<DataMapRow> ToDataMapRows();
}
