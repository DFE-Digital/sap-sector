using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SchoolViewModel(Establishment school)
{
    public string Name => school.EstablishmentName;
    public string Urn => school.URN;
    public string Ukprn => school.UKPRN.ToString();
    public string DfENumber => school.DfENumber;
}