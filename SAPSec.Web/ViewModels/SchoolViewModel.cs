using SAPSec.Infrastructure.Entities;

namespace SAPSec.Web.ViewModels;

public class SchoolViewModel(School school)
{
    public string Name => school.EstablishmentName;
    public string Urn => school.Urn.ToString();
    public string Ukprn => school.Ukprn.ToString();
    public string DfENumber => school.DfENumber;
}