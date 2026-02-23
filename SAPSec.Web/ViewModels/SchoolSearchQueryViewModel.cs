using System.ComponentModel.DataAnnotations;

namespace SAPSec.Web.ViewModels;

public class SchoolSearchQueryViewModel
{
    [Required(ErrorMessage = "Enter a school name or school ID to start a search")]
    [MinLength(3, ErrorMessage = "Enter a school name or school ID (minimum 3 characters)")]
    public string Query { get; set; } = string.Empty;

    public string? Urn { get; set; }

    public bool HasNoResults { get; set; }

    public bool SecondaryOnly { get; set; }

    public bool SimilarSchoolsOnly { get; set; }
}
