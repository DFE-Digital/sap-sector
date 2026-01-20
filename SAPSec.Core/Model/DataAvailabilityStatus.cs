using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model;

/// <summary>
/// Indicates the availability status of a data field.
/// Maps to GIAS data quality codes.
/// </summary>
public enum DataAvailabilityStatus
{
    /// <summary>Data is available and valid</summary>
    Available,

    /// <summary>Data has been redacted (GIAS code: c)</summary>
    Redacted,

    /// <summary>Data is not applicable for this school type (GIAS code: z)</summary>
    NotApplicable,

    /// <summary>Data is not available/missing (GIAS code: x)</summary>
    NotAvailable,

    /// <summary>Data quality is low/unreliable (GIAS code: low)</summary>
    Low
}