using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

public class SimilarSchoolsComparisonViewModel
{
    public List<SimilarSchoolViewModel> SimilarSchoolViewModels{ get; set;}
    public SchoolDetails comparedSchoolDetails { get; set; }
    
    public string DistanceBetweenSchools { get; set; } = "No available data";
}