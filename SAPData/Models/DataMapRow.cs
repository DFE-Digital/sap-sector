using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPData.Models
{

    public class DataMapRow
    {
        public string Range { get; set; } = "";               // e.g., SCHOOL
        public string Ref { get; set; } = "";               // e.g., 24_KS4_AT8_TOT
        public string PropertyName { get; set; } = "";
        public string PropertyDescription { get; set; } = "";
        public string Source { get; set; } = "";             // e.g., EES
        public string Type { get; set; } = "";           // e.g., KS4_Performance
        public string Subtype { get; set; } = "";            // e.g., Performance
        public string Year { get; set; } = "";               // e.g., 2024-2025
        public string YearDesc { get; set; } = "";
        public string FileName { get; set; } = "";           // e.g., 202425_performance_tables_schools_provisional.csv
        public string Field { get; set; } = "";              // e.g., attainment8_average
        public string DataType { get; set; } = "";           // e.g., int
        public string RecordFilterBy { get; set; } = "";     // e.g., URN
        public string Filter { get; set; } = "";             // e.g., breakdown
        public string FilterValue { get; set; } = "";        // e.g., Total
        public string Filter2 { get; set; } = "";            // optional
        public string Filter2Value { get; set; } = "";       // optional
        public string Filter3 { get; set; } = "";            // optional
        public string Filter3Value { get; set; } = "";       // optional

        public string Filter4 { get; set; } = "";            // optional
        public string Filter4Value { get; set; } = "";       // optional
        public string Filter5 { get; set; } = "";            // optional
        public string Filter5Value { get; set; } = "";       // optional
        public string ShouldBeNormalised { get; set; } = "";   // e.g., No -> false
        public string NormalisedLookup { get; set; } = "";   // optional
        public string CompoundFields { get; set; } = "";
        public string IgnoreMapping { get; set; } = "";

    }


    public class DataMapMapping : ClassMap<DataMapRow>
    {
        public DataMapMapping()
        {
            Map(m => m.Range).Name("Range");
            Map(m => m.Ref).Name("REF");
            Map(m => m.PropertyName).Name("PropertyName");
            Map(m => m.PropertyDescription).Name("PropertyDescription");
            Map(m => m.Source).Name("Source");
            Map(m => m.Type).Name("Type");
            Map(m => m.Subtype).Name("Subtype");
            Map(m => m.Year).Name("Year");
            Map(m => m.YearDesc).Name("YearDesc");
            Map(m => m.FileName).Name("File");
            Map(m => m.Field).Name("Field");
            Map(m => m.DataType).Name("DataType");
            Map(m => m.RecordFilterBy).Name("RecordFilterBy");
            Map(m => m.Filter).Name("Filter");
            Map(m => m.FilterValue).Name("FilterValue");
            Map(m => m.Filter2).Name("Filter2");
            Map(m => m.Filter2Value).Name("Filter2Value");
            Map(m => m.Filter3).Name("Filter3");
            Map(m => m.Filter3Value).Name("Filter3Value");
            Map(m => m.Filter4).Name("Filter4");
            Map(m => m.Filter4Value).Name("Filter4Value");
            Map(m => m.Filter5).Name("Filter5");
            Map(m => m.Filter5Value).Name("Filter5Value");
            Map(m => m.ShouldBeNormalised).Name("ShouldBeNormalised");
            Map(m => m.NormalisedLookup).Name("NormalisedLookup");
            Map(m => m.CompoundFields).Name("CompoundFields");
            Map(m => m.IgnoreMapping).Name("IgnoreMapping");
        }


    }

    public class DataMapLookup
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
