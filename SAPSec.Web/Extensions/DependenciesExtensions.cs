using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Destinations;
using SAPSec.Core.Interfaces.Services.KS4.Performance;
using SAPSec.Core.Interfaces.Services.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Services.KS4.Suspensions;
using SAPSec.Core.Interfaces.Services.KS4.Workforce;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Core.Model.KS4.Workforce;
using SAPSec.Core.Rules;
using SAPSec.Core.Services;
using SAPSec.Core.Services.KS4.Absence;
using SAPSec.Core.Services.KS4.Destinations;
using SAPSec.Core.Services.KS4.Performance;
using SAPSec.Core.Services.KS4.SubjectEntries;
using SAPSec.Core.Services.KS4.Suspensions;
using SAPSec.Core.Services.KS4.Workforce;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Infrastructure.Repositories;
using SAPSec.Infrastructure.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Absence;
using SAPSec.Infrastructure.Repositories.KS4.Destinations;
using SAPSec.Infrastructure.Repositories.KS4.Performance;
using SAPSec.Infrastructure.Repositories.KS4.SubjectEntries;
using SAPSec.Infrastructure.Repositories.KS4.Suspensions;
using SAPSec.Infrastructure.Repositories.KS4.Workforce;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DependenciesExtensions
    {
        public static void AddDependencies(this IServiceCollection services)
        {


            services.AddSingleton<IGenericRepository<Establishment>, JSONRepository<Establishment>>();
            services.AddSingleton<IEstablishmentRepository, EstablishmentRepository>();
            services.AddSingleton<IEstablishmentService, EstablishmentService>();

            services.AddSingleton<IGenericRepository<EstablishmentPerformance>, JSONRepository<EstablishmentPerformance>>();
            services.AddSingleton<IEstablishmentPerformanceRepository, EstablishmentPerformanceRepository>();
            services.AddSingleton<IEstablishmentPerformanceService, EstablishmentPerformanceService>();

            services.AddSingleton<IGenericRepository<EstablishmentDestinations>, JSONRepository<EstablishmentDestinations>>();
            services.AddSingleton<IEstablishmentDestinationsRepository, EstablishmentDestinationsRepository>();
            services.AddSingleton<IEstablishmentDestinationsService, EstablishmentDestinationsService>();

            services.AddSingleton<IGenericRepository<EstablishmentAbsence>, JSONRepository<EstablishmentAbsence>>();
            services.AddSingleton<IEstablishmentAbsenceRepository, EstablishmentAbsenceRepository>();
            services.AddSingleton<IEstablishmentAbsenceService, EstablishmentAbsenceService>();

            services.AddSingleton<IGenericRepository<EstablishmentWorkforce>, JSONRepository<EstablishmentWorkforce>>();
            services.AddSingleton<IEstablishmentWorkforceRepository, EstablishmentWorkforceRepository>();
            services.AddSingleton<IEstablishmentWorkforceService, EstablishmentWorkforceService>();

            services.AddSingleton<IGenericRepository<EstablishmentSuspensions>, JSONRepository<EstablishmentSuspensions>>();
            services.AddSingleton<IEstablishmentSuspensionsRepository, EstablishmentSuspensionsRepository>();
            services.AddSingleton<IEstablishmentSuspensionsService, EstablishmentSuspensionsService>();

            services.AddSingleton<IGenericRepository<EstablishmentSubjectEntries>, JSONRepository<EstablishmentSubjectEntries>>();
            services.AddSingleton<IEstablishmentSubjectEntriesRepository, EstablishmentSubjectEntriesRepository>();
            services.AddSingleton<IEstablishmentSubjectEntriesService, EstablishmentSubjectEntriesService>();


            services.AddSingleton<IGenericRepository<LAPerformance>, JSONRepository<LAPerformance>>();
            services.AddSingleton<ILAPerformanceRepository, LAPerformanceRepository>();
            services.AddSingleton<ILAPerformanceService, LAPerformanceService>();

            services.AddSingleton<IGenericRepository<LADestinations>, JSONRepository<LADestinations>>();
            services.AddSingleton<ILADestinationsRepository, LADestinationsRepository>();
            services.AddSingleton<ILADestinationsService, LADestinationsService>();

            services.AddSingleton<IGenericRepository<LASuspensions>, JSONRepository<LASuspensions>>();
            services.AddSingleton<ILASuspensionsRepository, LASuspensionsRepository>();
            services.AddSingleton<ILASuspensionsService, LASuspensionsService>();

            services.AddSingleton<IGenericRepository<LASubjectEntries>, JSONRepository<LASubjectEntries>>();
            services.AddSingleton<ILASubjectEntriesRepository, LASubjectEntriesRepository>();
            services.AddSingleton<ILASubjectEntriesService, LASubjectEntriesService>();

            services.AddSingleton<IGenericRepository<LAAbsence>, JSONRepository<LAAbsence>>();
            services.AddSingleton<ILAAbsenceRepository, LAAbsenceRepository>();
            services.AddSingleton<ILAAbsenceService, LAAbsenceService>();


            services.AddSingleton<IGenericRepository<EnglandPerformance>, JSONRepository<EnglandPerformance>>();
            services.AddSingleton<IEnglandPerformanceRepository, EnglandPerformanceRepository>();
            services.AddSingleton<IEnglandPerformanceService, EnglandPerformanceService>();

            services.AddSingleton<IGenericRepository<EnglandDestinations>, JSONRepository<EnglandDestinations>>();
            services.AddSingleton<IEnglandDestinationsRepository, EnglandDestinationsRepository>();
            services.AddSingleton<IEnglandDestinationsService, EnglandDestinationsService>();

            services.AddSingleton<IGenericRepository<EnglandSuspensions>, JSONRepository<EnglandSuspensions>>();
            services.AddSingleton<IEnglandSuspensionsRepository, EnglandSuspensionsRepository>();
            services.AddSingleton<IEnglandSuspensionsService, EnglandSuspensionsService>();

            services.AddSingleton<IGenericRepository<EnglandSubjectEntries>, JSONRepository<EnglandSubjectEntries>>();
            services.AddSingleton<IEnglandSubjectEntriesRepository, EnglandSubjectEntriesRepository>();
            services.AddSingleton<IEnglandSubjectEntriesService, EnglandSubjectEntriesService>();

            services.AddSingleton<IGenericRepository<EnglandAbsence>, JSONRepository<EnglandAbsence>>();
            services.AddSingleton<IEnglandAbsenceRepository, EnglandAbsenceRepository>();
            services.AddSingleton<IEnglandAbsenceService, EnglandAbsenceService>();

            services.AddSingleton<IGenericRepository<Lookup>, JSONRepository<Lookup>>();
            services.AddSingleton<ILookupRepository, LookupRepository>();
            services.AddSingleton<ILookupService, LookupService>();

            services.AddSingleton<ISearchRepository, LuceneSearchService>();

            services.AddSingleton<ISearchService, SearchService>();

            // Register SchoolDetailsService with explicit rule dependencies
            services.AddScoped<ISchoolDetailsService, SchoolDetailsService>();
        }
    }
}
