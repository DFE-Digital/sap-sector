using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.Destinations
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentDestinations
    {
        public string Id { get; set; } = string.Empty;
        /// 
        ///All Destinations Boys filtered by Establishment for Current year 
        /// 
        public double? AllDest_Boy_Est_Current_Pct { get; set; }
        public string AllDest_Boy_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by Establishment for Current year 
        /// 
        public double? AllDest_Dis_Est_Current_Pct { get; set; }
        public string AllDest_Dis_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by Establishment for Current year 
        /// 
        public double? AllDest_Grl_Est_Current_Pct { get; set; }
        public string AllDest_Grl_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by Establishment for Current year 
        /// 
        public double? AllDest_NDi_Est_Current_Pct { get; set; }
        public string AllDest_NDi_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by Establishment for Current year 
        /// 
        public double? AllDest_Tot_Est_Current_Pct { get; set; }
        public string AllDest_Tot_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by Establishment for Current year 
        /// 
        public double? Education_Boy_Est_Current_Pct { get; set; }
        public string Education_Boy_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by Establishment for Current year 
        /// 
        public double? Education_Dis_Est_Current_Pct { get; set; }
        public string Education_Dis_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by Establishment for Current year 
        /// 
        public double? Education_Grl_Est_Current_Pct { get; set; }
        public string Education_Grl_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by Establishment for Current year 
        /// 
        public double? Education_NDi_Est_Current_Pct { get; set; }
        public string Education_NDi_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by Establishment for Current year 
        /// 
        public double? Education_Tot_Est_Current_Pct { get; set; }
        public string Education_Tot_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by Establishment for Current year 
        /// 
        public double? Employment_Boy_Est_Current_Pct { get; set; }
        public string Employment_Boy_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by Establishment for Current year 
        /// 
        public double? Employment_Dis_Est_Current_Pct { get; set; }
        public string Employment_Dis_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by Establishment for Current year 
        /// 
        public double? Employment_Grl_Est_Current_Pct { get; set; }
        public string Employment_Grl_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by Establishment for Current year 
        /// 
        public double? Employment_NDi_Est_Current_Pct { get; set; }
        public string Employment_NDi_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by Establishment for Current year 
        /// 
        public double? Employment_Tot_Est_Current_Pct { get; set; }
        public string Employment_Tot_Est_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Boys filtered by Establishment for Previous year 
        /// 
        public double? AllDest_Boy_Est_Previous_Pct { get; set; }
        public string AllDest_Boy_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? AllDest_Dis_Est_Previous_Pct { get; set; }
        public string AllDest_Dis_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by Establishment for Previous year 
        /// 
        public double? AllDest_Grl_Est_Previous_Pct { get; set; }
        public string AllDest_Grl_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? AllDest_NDi_Est_Previous_Pct { get; set; }
        public string AllDest_NDi_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by Establishment for Previous year 
        /// 
        public double? AllDest_Tot_Est_Previous_Pct { get; set; }
        public string AllDest_Tot_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by Establishment for Previous year 
        /// 
        public double? Education_Boy_Est_Previous_Pct { get; set; }
        public string Education_Boy_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? Education_Dis_Est_Previous_Pct { get; set; }
        public string Education_Dis_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by Establishment for Previous year 
        /// 
        public double? Education_Grl_Est_Previous_Pct { get; set; }
        public string Education_Grl_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? Education_NDi_Est_Previous_Pct { get; set; }
        public string Education_NDi_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by Establishment for Previous year 
        /// 
        public double? Education_Tot_Est_Previous_Pct { get; set; }
        public string Education_Tot_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by Establishment for Previous year 
        /// 
        public double? Employment_Boy_Est_Previous_Pct { get; set; }
        public string Employment_Boy_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? Employment_Dis_Est_Previous_Pct { get; set; }
        public string Employment_Dis_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by Establishment for Previous year 
        /// 
        public double? Employment_Grl_Est_Previous_Pct { get; set; }
        public string Employment_Grl_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by Establishment for Previous year 
        /// 
        public double? Employment_NDi_Est_Previous_Pct { get; set; }
        public string Employment_NDi_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by Establishment for Previous year 
        /// 
        public double? Employment_Tot_Est_Previous_Pct { get; set; }
        public string Employment_Tot_Est_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Boys filtered by Establishment for Previous2 year 
        /// 
        public double? AllDest_Boy_Est_Previous2_Pct { get; set; }
        public string AllDest_Boy_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? AllDest_Dis_Est_Previous2_Pct { get; set; }
        public string AllDest_Dis_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by Establishment for Previous2 year 
        /// 
        public double? AllDest_Grl_Est_Previous2_Pct { get; set; }
        public string AllDest_Grl_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? AllDest_NDi_Est_Previous2_Pct { get; set; }
        public string AllDest_NDi_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by Establishment for Previous2 year 
        /// 
        public double? AllDest_Tot_Est_Previous2_Pct { get; set; }
        public string AllDest_Tot_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by Establishment for Previous2 year 
        /// 
        public double? Education_Boy_Est_Previous2_Pct { get; set; }
        public string Education_Boy_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? Education_Dis_Est_Previous2_Pct { get; set; }
        public string Education_Dis_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by Establishment for Previous2 year 
        /// 
        public double? Education_Grl_Est_Previous2_Pct { get; set; }
        public string Education_Grl_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? Education_NDi_Est_Previous2_Pct { get; set; }
        public string Education_NDi_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by Establishment for Previous2 year 
        /// 
        public double? Education_Tot_Est_Previous2_Pct { get; set; }
        public string Education_Tot_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by Establishment for Previous2 year 
        /// 
        public double? Employment_Boy_Est_Previous2_Pct { get; set; }
        public string Employment_Boy_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? Employment_Dis_Est_Previous2_Pct { get; set; }
        public string Employment_Dis_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by Establishment for Previous2 year 
        /// 
        public double? Employment_Grl_Est_Previous2_Pct { get; set; }
        public string Employment_Grl_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by Establishment for Previous2 year 
        /// 
        public double? Employment_NDi_Est_Previous2_Pct { get; set; }
        public string Employment_NDi_Est_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by Establishment for Previous2 year 
        /// 
        public double? Employment_Tot_Est_Previous2_Pct { get; set; }
        public string Employment_Tot_Est_Previous2_Pct_Reason { get; set; } = string.Empty;


    }
}
