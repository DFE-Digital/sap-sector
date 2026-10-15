namespace SAPSec.Core.Features.SchoolDetails;

/// <summary>
/// A school that a closed school's record signposts users to, resolved live from the current
/// establishment data (not the denormalised name on the link record) so the name and route are
/// always current.
/// </summary>
public sealed record SuccessorSchool(string Urn, string Name, string? PhaseOfEducationName);
