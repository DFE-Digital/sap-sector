using SAPPub.Core.Services.KS4.Absence;
using SAPPub.Core.Services.KS4.Destinations;
using SAPPub.Core.Services.KS4.Performance;
using SAPPub.Core.Services.KS4.Workforce;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Destinations;
using SAPSec.Core.Interfaces.Services.KS4.Performance;
using SAPSec.Core.Interfaces.Services.KS4.Workforce;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Model.KS4.Workforce;
using SAPSec.Core.Services;
using SAPSec.Core.Services.KS4.Performance;
using SAPSec.Infrastructure.LuceneSearch;
using SAPSec.Infrastructure.Repositories;
using SAPSec.Infrastructure.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Absence;
using SAPSec.Infrastructure.Repositories.KS4.Destinations;
using SAPSec.Infrastructure.Repositories.KS4.Performance;
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

            services.AddSingleton<IGenericRepository<LAPerformance>, JSONRepository<LAPerformance>>();
            services.AddSingleton<ILAPerformanceRepository, LAPerformanceRepository>();
            services.AddSingleton<ILAPerformanceService, LAPerformanceService>();

            services.AddSingleton<IGenericRepository<LADestinations>, JSONRepository<LADestinations>>();
            services.AddSingleton<ILADestinationsRepository, LADestinationsRepository>();
            services.AddSingleton<ILADestinationsService, LADestinationsService>();

            services.AddSingleton<IGenericRepository<EnglandPerformance>, JSONRepository<EnglandPerformance>>();
            services.AddSingleton<IEnglandPerformanceRepository, EnglandPerformanceRepository>();
            services.AddSingleton<IEnglandPerformanceService, EnglandPerformanceService>();

            services.AddSingleton<IGenericRepository<EnglandDestinations>, JSONRepository<EnglandDestinations>>();
            services.AddSingleton<IEnglandDestinationsRepository, EnglandDestinationsRepository>();
            services.AddSingleton<IEnglandDestinationsService, EnglandDestinationsService>();

            services.AddSingleton<ISearchRepository, LuceneSearchService>();

            services.AddSingleton<ISearchService, SearchService>();
        }
    }
}
