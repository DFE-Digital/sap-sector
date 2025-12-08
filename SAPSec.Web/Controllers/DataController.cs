using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Destinations;
using SAPSec.Core.Interfaces.Services.KS4.Performance;
using SAPSec.Core.Interfaces.Services.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Services.KS4.Suspensions;
using SAPSec.Core.Interfaces.Services.KS4.Workforce;
using SAPSec.Core.Model.KS4.SubjectEntries;

namespace SAPSec.Web.Controllers
{
    /// <summary>
    /// EXAMPLE CLASS, POC only.
    /// </summary>
    public class DataController : Controller
    {
        private IEstablishmentService _service;

        private IEstablishmentAbsenceService _absenceService;
        private IEstablishmentPerformanceService _performanceService;
        private IEstablishmentDestinationsService _destinationsService;
        private IEstablishmentSubjectEntriesService _subjectEntriesService;
        private IEstablishmentWorkforceService _workforceService;
        private IEstablishmentSuspensionsService _suspensionsService;

        private ILADestinationsService _LAdestinationsService;
        private ILAPerformanceService _LAperformanceService;
        private ILASubjectEntriesService _LASubjectEntries;
        private ILAAbsenceService _LAAbsenceService;
        private ILASuspensionsService _LASuspensionsService;

        private IEnglandDestinationsService _englandDestinationsService;
        private IEnglandPerformanceService _englandPerformanceService;
        private IEnglandAbsenceService _englandAbsenceService;
        private IEnglandSubjectEntriesService _englandSubjectEntriesService;
        private IEnglandSuspensionsService _englandSuspensionsService;


        public DataController(
            IEstablishmentService establishmentService,
            IEstablishmentAbsenceService absenceService,
            IEstablishmentPerformanceService performanceService,
            IEstablishmentDestinationsService destinationsService,
            IEstablishmentWorkforceService workforceService,
            IEstablishmentSuspensionsService suspensionsService,
            ILADestinationsService ladestinationsService,
            ILAPerformanceService laperformanceService,
            ILASubjectEntriesService lasubjectEntriesService,
            ILAAbsenceService laAbsenceService,
            ILASuspensionsService lasuspensionsService,
            IEnglandDestinationsService englandDestinationsService,
            IEnglandPerformanceService englandPerformanceService,
            IEnglandAbsenceService englandAbsenceService,
            IEnglandSubjectEntriesService englandSubjectEntriesService,
            IEstablishmentSubjectEntriesService subjectEntriesService,
            IEnglandSuspensionsService englandSuspensionsService
            )
        {

            _service = establishmentService;

            _absenceService = absenceService;
            _performanceService = performanceService;
            _destinationsService = destinationsService;
            _workforceService = workforceService;
            _subjectEntriesService = subjectEntriesService;
            _suspensionsService = suspensionsService;
            _LASuspensionsService = lasuspensionsService;
            _englandSuspensionsService = englandSuspensionsService;

            _LAAbsenceService = laAbsenceService;
            _LASubjectEntries = lasubjectEntriesService;
            _LAdestinationsService = ladestinationsService;
            _LAperformanceService = laperformanceService;
            _englandDestinationsService = englandDestinationsService;
            _englandPerformanceService = englandPerformanceService;
            _englandAbsenceService = englandAbsenceService;
            _englandSubjectEntriesService = englandSubjectEntriesService;

        }

        public IActionResult Index()
        {
            var data = _service.GetAllEstablishments();
            return View(data);
        }

        public IActionResult Data(string id)
        {
            //This could be wrapped into a ViewModel. Not in scope for this delivery though.

            var dataModel = _service.GetEstablishment(id);

            dataModel.EstablishmentAbsence = _absenceService.GetEstablishmentAbsence(dataModel.URN);
            dataModel.LAAbsence = _LAAbsenceService.GetLAAbsence(dataModel.LAId?.ToString());
            dataModel.EnglandAbsence = _englandAbsenceService.GetEnglandAbsence();

            dataModel.KS4SubjectEntries = _subjectEntriesService.GetEstablishmentSubjectEntries(id);
            dataModel.LASubjectEntries = _LASubjectEntries.GetLASubjectEntries(dataModel.LAId?.ToString());
            dataModel.EnglandSubjectEntries = _englandSubjectEntriesService.GetEnglandSubjectEntries();

            dataModel.KS4Performance = _performanceService.GetEstablishmentPerformance(id);
            dataModel.LAPerformance = _LAperformanceService.GetLAPerformance(dataModel.LAId?.ToString());
            dataModel.EnglandPerformance = _englandPerformanceService.GetEnglandPerformance();

            dataModel.EstablishmentDestinations = _destinationsService.GetEstablishmentDestinations(id);
            dataModel.LADestinations = _LAdestinationsService.GetLADestinations(dataModel.LAId?.ToString());
            dataModel.EnglandDestinations = _englandDestinationsService.GetEnglandDestinations();

            dataModel.EstablishmentSuspensions = _suspensionsService.GetEstablishmentSuspensions(id);
            dataModel.LASuspensions = _LASuspensionsService.GetLASuspensions(dataModel.LAId?.ToString());
            dataModel.EnglandSuspensions = _englandSuspensionsService.GetEnglandSuspensions();  


            dataModel.Workforce = _workforceService.GetEstablishmentWorkforce(id);

            return View(dataModel);

        }
    }
}
