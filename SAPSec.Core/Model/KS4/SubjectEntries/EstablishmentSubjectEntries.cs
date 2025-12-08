using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model.KS4.SubjectEntries
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentSubjectEntries
    {
        public string Id { get; set; } = string.Empty;
        /// 
        ///Biology grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Bio49_Sum_Est_Current_Num { get; set; }
        public string Bio49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Bio59_Sum_Est_Current_Num { get; set; }
        public string Bio59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Bio79_Sum_Est_Current_Num { get; set; }
        public string Bio79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Chem49_Sum_Est_Current_Num { get; set; }
        public string Chem49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Chem59_Sum_Est_Current_Num { get; set; }
        public string Chem59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Chem79_Sum_Est_Current_Num { get; set; }
        public string Chem79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? CombSci49_Sum_Est_Current_Num { get; set; }
        public string CombSci49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? CombSci59_Sum_Est_Current_Num { get; set; }
        public string CombSci59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? CombSci79_Sum_Est_Current_Num { get; set; }
        public string CombSci79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLang49_Sum_Est_Current_Num { get; set; }
        public string EngLang49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLang59_Sum_Est_Current_Num { get; set; }
        public string EngLang59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLang79_Sum_Est_Current_Num { get; set; }
        public string EngLang79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLit49_Sum_Est_Current_Num { get; set; }
        public string EngLit49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLit59_Sum_Est_Current_Num { get; set; }
        public string EngLit59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? EngLit79_Sum_Est_Current_Num { get; set; }
        public string EngLit79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Maths49_Sum_Est_Current_Num { get; set; }
        public string Maths49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Maths59_Sum_Est_Current_Num { get; set; }
        public string Maths59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Maths79_Sum_Est_Current_Num { get; set; }
        public string Maths79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 4 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Physics49_Sum_Est_Current_Num { get; set; }
        public string Physics49_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 5 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Physics59_Sum_Est_Current_Num { get; set; }
        public string Physics59_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 7 to 9 Sum filtered by Establishment for Current year 
        /// 
        public double? Physics79_Sum_Est_Current_Num { get; set; }
        public string Physics79_Sum_Est_Current_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Bio49_Sum_Est_Previous_Num { get; set; }
        public string Bio49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Bio59_Sum_Est_Previous_Num { get; set; }
        public string Bio59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Bio79_Sum_Est_Previous_Num { get; set; }
        public string Bio79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Chem49_Sum_Est_Previous_Num { get; set; }
        public string Chem49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Chem59_Sum_Est_Previous_Num { get; set; }
        public string Chem59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Chem79_Sum_Est_Previous_Num { get; set; }
        public string Chem79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? CombSci49_Sum_Est_Previous_Num { get; set; }
        public string CombSci49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? CombSci59_Sum_Est_Previous_Num { get; set; }
        public string CombSci59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? CombSci79_Sum_Est_Previous_Num { get; set; }
        public string CombSci79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLang49_Sum_Est_Previous_Num { get; set; }
        public string EngLang49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLang59_Sum_Est_Previous_Num { get; set; }
        public string EngLang59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLang79_Sum_Est_Previous_Num { get; set; }
        public string EngLang79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLit49_Sum_Est_Previous_Num { get; set; }
        public string EngLit49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLit59_Sum_Est_Previous_Num { get; set; }
        public string EngLit59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? EngLit79_Sum_Est_Previous_Num { get; set; }
        public string EngLit79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Maths49_Sum_Est_Previous_Num { get; set; }
        public string Maths49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Maths59_Sum_Est_Previous_Num { get; set; }
        public string Maths59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Maths79_Sum_Est_Previous_Num { get; set; }
        public string Maths79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 4 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Physics49_Sum_Est_Previous_Num { get; set; }
        public string Physics49_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 5 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Physics59_Sum_Est_Previous_Num { get; set; }
        public string Physics59_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 7 to 9 Sum filtered by Establishment for Previous year 
        /// 
        public double? Physics79_Sum_Est_Previous_Num { get; set; }
        public string Physics79_Sum_Est_Previous_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Bio49_Sum_Est_Previous2_Num { get; set; }
        public string Bio49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Bio59_Sum_Est_Previous2_Num { get; set; }
        public string Bio59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Biology grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Bio79_Sum_Est_Previous2_Num { get; set; }
        public string Bio79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Chem49_Sum_Est_Previous2_Num { get; set; }
        public string Chem49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Chem59_Sum_Est_Previous2_Num { get; set; }
        public string Chem59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Chemistry grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Chem79_Sum_Est_Previous2_Num { get; set; }
        public string Chem79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? CombSci49_Sum_Est_Previous2_Num { get; set; }
        public string CombSci49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? CombSci59_Sum_Est_Previous2_Num { get; set; }
        public string CombSci59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Combined Science (Dual Award) grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? CombSci79_Sum_Est_Previous2_Num { get; set; }
        public string CombSci79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLang49_Sum_Est_Previous2_Num { get; set; }
        public string EngLang49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLang59_Sum_Est_Previous2_Num { get; set; }
        public string EngLang59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Language grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLang79_Sum_Est_Previous2_Num { get; set; }
        public string EngLang79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLit49_Sum_Est_Previous2_Num { get; set; }
        public string EngLit49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLit59_Sum_Est_Previous2_Num { get; set; }
        public string EngLit59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///English Literature grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? EngLit79_Sum_Est_Previous2_Num { get; set; }
        public string EngLit79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Maths49_Sum_Est_Previous2_Num { get; set; }
        public string Maths49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Maths59_Sum_Est_Previous2_Num { get; set; }
        public string Maths59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Maths grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Maths79_Sum_Est_Previous2_Num { get; set; }
        public string Maths79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 4 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Physics49_Sum_Est_Previous2_Num { get; set; }
        public string Physics49_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 5 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Physics59_Sum_Est_Previous2_Num { get; set; }
        public string Physics59_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;

        /// 
        ///Physics grades 7 to 9 Sum filtered by Establishment for Previous2 year 
        /// 
        public double? Physics79_Sum_Est_Previous2_Num { get; set; }
        public string Physics79_Sum_Est_Previous2_Num_Reason { get; set; } = string.Empty;


    }
}
