using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.Workforce
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentWorkforce
    {
        public string Id { get; set; } = string.Empty;
        /// 
        /// Workforce Pupil Teacher Ratio filtered by Establishment for Current year 
        /// 
        public double? Workforce_PupTeaRatio_Est_Current_Num { get; set; }
        public string Workforce_PupTeaRatio_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        /// Workforce PupIl FTE filtered by Establishment for Current year 
        /// 
        public double? Workforce_TotPupils_Est_Current_Num { get; set; }
        public string Workforce_TotPupils_Est_Current_Num_Reason { get; set; } = string.Empty;


    }
}
