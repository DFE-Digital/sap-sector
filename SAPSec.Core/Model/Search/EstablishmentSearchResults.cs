using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.Search
{
    [ExcludeFromCodeCoverage]
    public record EstablishmentSearchResult
    (
        string Name,
        Establishment Establishment
    );
}
