// Verified against a local app instance running with ASPNETCORE_ENVIRONMENT=LoadTest
// (JSON-backed repositories, see SAPSec.Infrastructure/Data/Files/Generated/Establishment.json).
// Sourced from SAPSec.Infrastructure/Data/Files/TestEstablishmentUrns.json and confirmed to
// return 200 from their respective overview pages.

export const secondarySchoolUrns = [
  '105574', '105576', '105581', '107140', '108055', '108057', '108058', '108075',
  '108079', '108085', '108088', '131880', '131895', '131896', '134224', '135874',
  '135934', '135935', '136105', '136174', '136343', '136392', '136826', '137065',
  '137083', '137309', '137383', '137577', '137704', '137775'
]

// Primary-phase schools only (PhaseOfEducationName = 'Primary'). The other URNs in
// TestEstablishmentUrns.json are All-through schools, which the /school/primary/...
// routes 404 on unless EnableAllThroughSchools is on.
export const primarySchoolUrns = [
  '101193', '101206', '101230', '109567', '136413'
]

export function randomSecondaryUrn () {
  return secondarySchoolUrns[Math.floor(Math.random() * secondarySchoolUrns.length)]
}

export function randomPrimaryUrn () {
  return primarySchoolUrns[Math.floor(Math.random() * primarySchoolUrns.length)]
}
