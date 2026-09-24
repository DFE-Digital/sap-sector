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

export const primarySchoolUrns = [
  '100590', '100747', '100749', '101193', '101206', '101230', '101243', '103080',
  '107562', '109567', '112951', '116407', '125279', '134369', '135264', '135296',
  '135367', '135423', '135507', '135563', '135584', '135597', '135619', '135866',
  '135958', '135966', '136413', '136454'
]

export function randomSecondaryUrn () {
  return secondarySchoolUrns[Math.floor(Math.random() * secondarySchoolUrns.length)]
}

export function randomPrimaryUrn () {
  return primarySchoolUrns[Math.floor(Math.random() * primarySchoolUrns.length)]
}
