using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class Lookup
    {
        public string Id { get; set; } = string.Empty;
        public string LookupType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
