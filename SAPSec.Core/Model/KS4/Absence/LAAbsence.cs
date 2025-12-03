using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.Absence
{
    [ExcludeFromCodeCoverage]
    public class LAAbsence
    {
        public string Id { get; set; } = string.Empty;
        /// 
        ///Absence Persistent filtered by LA for Current year 
        /// 
        public double? Abs_Persistent_LA_Current_Pct { get; set; }
        public string Abs_Persistent_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Persistent filtered by LA for Previous year 
        /// 
        public double? Abs_Persistent_LA_Previous_Pct { get; set; }
        public string Abs_Persistent_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Persistent filtered by LA for Previous2 year 
        /// 
        public double? Abs_Persistent_LA_Previous2_Pct { get; set; }
        public string Abs_Persistent_LA_Previous2_Pct_Reason { get; set; } = string.Empty;


    }
}
