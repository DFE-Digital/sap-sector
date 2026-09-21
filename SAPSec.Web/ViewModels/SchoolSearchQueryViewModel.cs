using System.ComponentModel.DataAnnotations;

namespace SAPSec.Web.ViewModels;

public class SchoolSearchQueryViewModel
{
    [Required(ErrorMessage = "Enter a school name (minimum 3 characters), URN, DfE number or UKPRN")]
    [MinLength(3, ErrorMessage = "Enter a school name (minimum 3 characters), URN, DfE number or UKPRN")]
    public string Query { get; set; } = string.Empty;

    public string? Urn { get; set; }

    public bool HasNoResults { get; set; }
}
