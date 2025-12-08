using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.Absence
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentAbsence
    {
        public string Id { get; set; } = string.Empty;
        /// 
        ///Absence Total filtered by Establishment for Current year 
        /// 
        public double? Abs_Tot_Est_Current_Pct { get; set; }
        public string Abs_Tot_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Persistent filtered by Establishment for Current year 
        /// 
        public double? Abs_Persistent_Est_Current_Pct { get; set; }
        public string Abs_Persistent_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Total filtered by Establishment for Previous year 
        /// 
        public double? Abs_Tot_Est_Previous_Pct { get; set; }
        public string Abs_Tot_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Persistent filtered by Establishment for Previous year 
        /// 
        public double? Abs_Persistent_Est_Previous_Pct { get; set; }
        public string Abs_Persistent_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Total filtered by Establishment for Previous2 year 
        /// 
        public double? Abs_Tot_Est_Previous2_Pct { get; set; }
        public string Abs_Tot_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///Absence Persistent filtered by Establishment for Previous2 year 
        /// 
        public double? Abs_Persistent_Est_Previous2_Pct { get; set; }
        public string Abs_Persistent_Est_Previous2_Pct_Reason { get; set; } = string.Empty;


    }
}
