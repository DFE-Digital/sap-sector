using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.Destinations
{
    [ExcludeFromCodeCoverage]
    public class LADestinations
    {
        public string Id { get; set; } = string.Empty;
        /// 
        ///All Destinations Boys filtered by LA for Current year 
        /// 
        public double? AllDest_Boy_LA_Current_Pct { get; set; }
        public string AllDest_Boy_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by LA for Current year 
        /// 
        public double? AllDest_Dis_LA_Current_Pct { get; set; }
        public string AllDest_Dis_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by LA for Current year 
        /// 
        public double? AllDest_Grl_LA_Current_Pct { get; set; }
        public string AllDest_Grl_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by LA for Current year 
        /// 
        public double? AllDest_NDi_LA_Current_Pct { get; set; }
        public string AllDest_NDi_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by LA for Current year 
        /// 
        public double? AllDest_Tot_LA_Current_Pct { get; set; }
        public string AllDest_Tot_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by LA for Current year 
        /// 
        public double? Education_Boy_LA_Current_Pct { get; set; }
        public string Education_Boy_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by LA for Current year 
        /// 
        public double? Education_Dis_LA_Current_Pct { get; set; }
        public string Education_Dis_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by LA for Current year 
        /// 
        public double? Education_Grl_LA_Current_Pct { get; set; }
        public string Education_Grl_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by LA for Current year 
        /// 
        public double? Education_NDi_LA_Current_Pct { get; set; }
        public string Education_NDi_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by LA for Current year 
        /// 
        public double? Education_Tot_LA_Current_Pct { get; set; }
        public string Education_Tot_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by LA for Current year 
        /// 
        public double? Employment_Boy_LA_Current_Pct { get; set; }
        public string Employment_Boy_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by LA for Current year 
        /// 
        public double? Employment_Dis_LA_Current_Pct { get; set; }
        public string Employment_Dis_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by LA for Current year 
        /// 
        public double? Employment_Grl_LA_Current_Pct { get; set; }
        public string Employment_Grl_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by LA for Current year 
        /// 
        public double? Employment_NDi_LA_Current_Pct { get; set; }
        public string Employment_NDi_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by LA for Current year 
        /// 
        public double? Employment_Tot_LA_Current_Pct { get; set; }
        public string Employment_Tot_LA_Current_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Boys filtered by LA for Previous year 
        /// 
        public double? AllDest_Boy_LA_Previous_Pct { get; set; }
        public string AllDest_Boy_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by LA for Previous year 
        /// 
        public double? AllDest_Dis_LA_Previous_Pct { get; set; }
        public string AllDest_Dis_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by LA for Previous year 
        /// 
        public double? AllDest_Grl_LA_Previous_Pct { get; set; }
        public string AllDest_Grl_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by LA for Previous year 
        /// 
        public double? AllDest_NDi_LA_Previous_Pct { get; set; }
        public string AllDest_NDi_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by LA for Previous year 
        /// 
        public double? AllDest_Tot_LA_Previous_Pct { get; set; }
        public string AllDest_Tot_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by LA for Previous year 
        /// 
        public double? Education_Boy_LA_Previous_Pct { get; set; }
        public string Education_Boy_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by LA for Previous year 
        /// 
        public double? Education_Dis_LA_Previous_Pct { get; set; }
        public string Education_Dis_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by LA for Previous year 
        /// 
        public double? Education_Grl_LA_Previous_Pct { get; set; }
        public string Education_Grl_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by LA for Previous year 
        /// 
        public double? Education_NDi_LA_Previous_Pct { get; set; }
        public string Education_NDi_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by LA for Previous year 
        /// 
        public double? Education_Tot_LA_Previous_Pct { get; set; }
        public string Education_Tot_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by LA for Previous year 
        /// 
        public double? Employment_Boy_LA_Previous_Pct { get; set; }
        public string Employment_Boy_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by LA for Previous year 
        /// 
        public double? Employment_Dis_LA_Previous_Pct { get; set; }
        public string Employment_Dis_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by LA for Previous year 
        /// 
        public double? Employment_Grl_LA_Previous_Pct { get; set; }
        public string Employment_Grl_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by LA for Previous year 
        /// 
        public double? Employment_NDi_LA_Previous_Pct { get; set; }
        public string Employment_NDi_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by LA for Previous year 
        /// 
        public double? Employment_Tot_LA_Previous_Pct { get; set; }
        public string Employment_Tot_LA_Previous_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Boys filtered by LA for Previous2 year 
        /// 
        public double? AllDest_Boy_LA_Previous2_Pct { get; set; }
        public string AllDest_Boy_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? AllDest_Dis_LA_Previous2_Pct { get; set; }
        public string AllDest_Dis_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Girls filtered by LA for Previous2 year 
        /// 
        public double? AllDest_Grl_LA_Previous2_Pct { get; set; }
        public string AllDest_Grl_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Not disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? AllDest_NDi_LA_Previous2_Pct { get; set; }
        public string AllDest_NDi_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Destinations Total filtered by LA for Previous2 year 
        /// 
        public double? AllDest_Tot_LA_Previous2_Pct { get; set; }
        public string AllDest_Tot_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Boys filtered by LA for Previous2 year 
        /// 
        public double? Education_Boy_LA_Previous2_Pct { get; set; }
        public string Education_Boy_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? Education_Dis_LA_Previous2_Pct { get; set; }
        public string Education_Dis_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Girls filtered by LA for Previous2 year 
        /// 
        public double? Education_Grl_LA_Previous2_Pct { get; set; }
        public string Education_Grl_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Not disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? Education_NDi_LA_Previous2_Pct { get; set; }
        public string Education_NDi_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Education Total filtered by LA for Previous2 year 
        /// 
        public double? Education_Tot_LA_Previous2_Pct { get; set; }
        public string Education_Tot_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Boys filtered by LA for Previous2 year 
        /// 
        public double? Employment_Boy_LA_Previous2_Pct { get; set; }
        public string Employment_Boy_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? Employment_Dis_LA_Previous2_Pct { get; set; }
        public string Employment_Dis_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Girls filtered by LA for Previous2 year 
        /// 
        public double? Employment_Grl_LA_Previous2_Pct { get; set; }
        public string Employment_Grl_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Not disadvantaged filtered by LA for Previous2 year 
        /// 
        public double? Employment_NDi_LA_Previous2_Pct { get; set; }
        public string Employment_NDi_LA_Previous2_Pct_Reason { get; set; } = string.Empty;

        /// 
        ///All Employment Total filtered by LA for Previous2 year 
        /// 
        public double? Employment_Tot_LA_Previous2_Pct { get; set; }
        public string Employment_Tot_LA_Previous2_Pct_Reason { get; set; } = string.Empty;


    }
}
