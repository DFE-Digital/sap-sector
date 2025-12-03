using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Model
{
    [ExcludeFromCodeCoverage]
    public class Establishment
    {
        public string URN { get; set; } = string.Empty;
        public string LAId { get; set; } = string.Empty;

        public EstablishmentMetadata Metadata { get; set; } = new();

        public EstablishmentPerformance KS4Performance { get; set; } = new();
        public LAPerformance LAPerformance { get; set; } = new();
        public EnglandPerformance EnglandPerformance { get; set; } = new();

        public EstablishmentSubjectEntries KS4SubjectEntries { get; set; } = new();
        public LASubjectEntries LASubjectEntries { get; set; } = new();
        public EnglandSubjectEntries EnglandSubjectEntries { get; set; } = new();

        public EstablishmentDestinations EstablishmentDestinations { get; set; } = new();
        public LADestinations LADestinations { get; set; } = new();
        public EnglandDestinations EnglandDestinations { get; set; } = new();


        public EstablishmentAbsence EstablishmentAbsence { get; set; } = new();
        public LAAbsence LAAbsence { get; set; } = new();
        public EnglandAbsence Absence { get; set; } = new(); // Will eventually need one per phase

        public EstablishmentWorkforce Workforce { get; set; } = new(); // Will eventually need one per phase



    }
}
