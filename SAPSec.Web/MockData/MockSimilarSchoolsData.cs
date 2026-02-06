using SAPSec.Web.ViewModels;

namespace SAPSec.Web.MockData;

/// <summary>
/// Temporary mock data for similar schools.
/// TODO: Replace with actual database queries against v_similar_schools_secondary_groups,
/// v_similar_schools_secondary_values, and v_establishment views.
/// </summary>
public static class MockSimilarSchoolsData
{
    public static List<SimilarSchoolViewModel> GetSimilarSchools(int urn)
    {
        return new List<SimilarSchoolViewModel>
        {
            // Rank 1-10
            new()
            {
                Urn = urn, NeighbourUrn = 100750, Dist = 0.3573, Rank = 1,
                Ks2Rp = 107.67, Ks2Mp = 106.23, PpPerc = 23.29, PercentEal = 30.77,
                Polar4QuintilePupils = 4, PStability = 90.21, IdaciPupils = 0.201,
                PercentSchSupport = 14.73, NumberOfPupils = 958, PercentStatementOrEhp = 1.57,
                Att8Scr = 60.7,
                EstablishmentName = "Abbey Academy",
                Street = "Abbey Way", Town = "Sheffield", County = "South Yorkshire", Postcode = "S17 5DE",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
                Region = "Yorkshire and The Humber", UrbanOrRural = "Urban",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 1200, OverallAbsenceRate = 5.2, PersistentAbsenceRate = 12.1
            },
            new()
            {
                Urn = urn, NeighbourUrn = 102153, Dist = 0.3646, Rank = 2,
                Ks2Rp = 106.89, Ks2Mp = 105.41, PpPerc = 28.15, PercentEal = 25.60,
                Polar4QuintilePupils = 4, PStability = 91.10, IdaciPupils = 0.195,
                PercentSchSupport = 12.80, NumberOfPupils = 1120, PercentStatementOrEhp = 1.42,
                Att8Scr = 53.2,
                EstablishmentName = "Wentworth High School",
                Street = "Sycamore Rise", Town = "Bakewell", County = "Derbyshire", Postcode = "DE45 1GH",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Foundation school",
                Region = "East Midlands", UrbanOrRural = "Rural",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 800, OverallAbsenceRate = 4.8, PersistentAbsenceRate = 10.3
            },
            new()
            {
                Urn = urn, NeighbourUrn = 137442, Dist = 0.3892, Rank = 3,
                Ks2Rp = 105.22, Ks2Mp = 104.88, PpPerc = 35.40, PercentEal = 18.90,
                Polar4QuintilePupils = 3, PStability = 88.50, IdaciPupils = 0.220,
                PercentSchSupport = 15.10, NumberOfPupils = 780, PercentStatementOrEhp = 2.10,
                Att8Scr = 43.5,
                EstablishmentName = "Oakley Dale Secondary School",
                Street = "Elmwood Lane", Town = "Keswick", County = "Cumbria", Postcode = "CA12 4RP",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school",
                Region = "North West", UrbanOrRural = "Rural",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "Resourced provision",
                SchoolCapacity = 600, OverallAbsenceRate = 6.1, PersistentAbsenceRate = 14.2
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100741, Dist = 0.4231, Rank = 4,
                Ks2Rp = 108.10, Ks2Mp = 106.50, PpPerc = 20.80, PercentEal = 32.10,
                Polar4QuintilePupils = 4, PStability = 92.30, IdaciPupils = 0.188,
                PercentSchSupport = 11.50, NumberOfPupils = 1350, PercentStatementOrEhp = 1.20,
                Att8Scr = 47.0,
                EstablishmentName = "Moorfield C of E Secondary School",
                Street = "Rosehill Avenue", Town = "Bury St Edmunds", County = "Suffolk", Postcode = "IP33 2QW",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Voluntary aided school",
                Region = "East of England", UrbanOrRural = "Urban",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 950, OverallAbsenceRate = 4.5, PersistentAbsenceRate = 9.8
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100279, Dist = 0.4300, Rank = 5,
                Ks2Rp = 104.50, Ks2Mp = 103.90, PpPerc = 42.30, PercentEal = 45.20,
                Polar4QuintilePupils = 3, PStability = 86.70, IdaciPupils = 0.265,
                PercentSchSupport = 16.80, NumberOfPupils = 1080, PercentStatementOrEhp = 2.50,
                Att8Scr = 42.1,
                EstablishmentName = "Excellence Academy",
                Street = "Maple Grove", Town = "Alnwick", County = "Northumberland", Postcode = "NE66 1AB",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
                Region = "North East", UrbanOrRural = "Rural",
                AdmissionsPolicy = "Non-selective", Gender = "Mixed",
                HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 700, OverallAbsenceRate = 5.8, PersistentAbsenceRate = 13.5
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100280, Dist = 0.4412, Rank = 6,
                Ks2Rp = 109.20, Ks2Mp = 107.80, PpPerc = 18.50, PercentEal = 12.30,
                Polar4QuintilePupils = 5, PStability = 93.40, IdaciPupils = 0.152,
                PercentSchSupport = 10.20, NumberOfPupils = 1100, PercentStatementOrEhp = 0.95,
                Att8Scr = 57.3,
                EstablishmentName = "Turn Style Academy Secondary",
                Street = "Hawthorn Drive", Town = "Lymington", County = "Hampshire", Postcode = "SO41 8DN",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
                Region = "South East", UrbanOrRural = "Urban",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = true, ResourcedProvisionType = "None",
                SchoolCapacity = 1100, OverallAbsenceRate = 3.9, PersistentAbsenceRate = 8.2
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100281, Dist = 0.4520, Rank = 7,
                Ks2Rp = 106.30, Ks2Mp = 105.90, PpPerc = 30.20, PercentEal = 8.50,
                Polar4QuintilePupils = 3, PStability = 89.80, IdaciPupils = 0.198,
                PercentSchSupport = 13.90, NumberOfPupils = 850, PercentStatementOrEhp = 1.80,
                Att8Scr = 59.0,
                EstablishmentName = "Nook Lane Secondary School",
                Street = "Cedar Way", Town = "Malvern", County = "Worcestershire", Postcode = "WR14 3PL",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school",
                Region = "West Midlands", UrbanOrRural = "Rural",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "SEN unit",
                SchoolCapacity = 850, OverallAbsenceRate = 4.2, PersistentAbsenceRate = 9.1
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100282, Dist = 0.4635, Rank = 8,
                Ks2Rp = 105.80, Ks2Mp = 104.20, PpPerc = 33.60, PercentEal = 22.40,
                Polar4QuintilePupils = 3, PStability = 87.90, IdaciPupils = 0.230,
                PercentSchSupport = 15.60, NumberOfPupils = 750, PercentStatementOrEhp = 2.30,
                Att8Scr = 47.5,
                EstablishmentName = "Rushley Mead Secondary School",
                Street = "Birchfield Road", Town = "Hexham", County = "Northumberland", Postcode = "NE46 3JS",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Foundation school",
                Region = "North East", UrbanOrRural = "Rural",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 750, OverallAbsenceRate = 5.5, PersistentAbsenceRate = 12.8
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100283, Dist = 0.4750, Rank = 9,
                Ks2Rp = 104.80, Ks2Mp = 103.50, PpPerc = 38.90, PercentEal = 15.70,
                Polar4QuintilePupils = 3, PStability = 88.20, IdaciPupils = 0.245,
                PercentSchSupport = 14.20, NumberOfPupils = 900, PercentStatementOrEhp = 2.80,
                Att8Scr = 39.2,
                EstablishmentName = "Chestnuts Secondary School",
                Street = "Ashdown Close", Town = "Tonbridge", County = "Kent", Postcode = "TN9 1LT",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
                Region = "South East", UrbanOrRural = "Urban",
                AdmissionsPolicy = "Non-selective", Gender = "Mixed",
                HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 900, OverallAbsenceRate = 6.8, PersistentAbsenceRate = 15.9
            },
            new()
            {
                Urn = urn, NeighbourUrn = 100284, Dist = 0.4880, Rank = 10,
                Ks2Rp = 107.50, Ks2Mp = 106.10, PpPerc = 25.70, PercentEal = 28.90,
                Polar4QuintilePupils = 4, PStability = 91.50, IdaciPupils = 0.190,
                PercentSchSupport = 12.10, NumberOfPupils = 1050, PercentStatementOrEhp = 1.35,
                Att8Scr = 51.4,
                EstablishmentName = "Greenfield Park Academy",
                Street = "Willow Road", Town = "Harrogate", County = "North Yorkshire", Postcode = "HG1 4PQ",
                PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy",
                Region = "Yorkshire and The Humber", UrbanOrRural = "Urban",
                AdmissionsPolicy = "Comprehensive", Gender = "Mixed",
                HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None",
                SchoolCapacity = 1050, OverallAbsenceRate = 4.6, PersistentAbsenceRate = 10.5
            },

            // Rank 11-20
            new() { Urn = urn, NeighbourUrn = 100285, Dist = 0.4950, Rank = 11, Ks2Rp = 106.10, Ks2Mp = 105.30, PpPerc = 31.40, PercentEal = 19.80, Polar4QuintilePupils = 3, PStability = 89.10, IdaciPupils = 0.215, PercentSchSupport = 14.50, NumberOfPupils = 880, PercentStatementOrEhp = 1.90, Att8Scr = 44.8, EstablishmentName = "Brookside Community School", Street = "Riverside Walk", Town = "Shrewsbury", County = "Shropshire", Postcode = "SY1 2AB", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "West Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = true, ResourcedProvisionType = "Resourced provision", SchoolCapacity = 880, OverallAbsenceRate = 5.4, PersistentAbsenceRate = 11.7 },
            new() { Urn = urn, NeighbourUrn = 100286, Dist = 0.5020, Rank = 12, Ks2Rp = 108.50, Ks2Mp = 107.20, PpPerc = 22.10, PercentEal = 10.50, Polar4QuintilePupils = 4, PStability = 92.80, IdaciPupils = 0.168, PercentSchSupport = 11.30, NumberOfPupils = 700, PercentStatementOrEhp = 1.10, Att8Scr = 55.6, EstablishmentName = "St Margaret's High School", Street = "Church Lane", Town = "Exeter", County = "Devon", Postcode = "EX1 3TD", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Voluntary aided school", Region = "South West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Girls", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 700, OverallAbsenceRate = 3.7, PersistentAbsenceRate = 7.9 },
            new() { Urn = urn, NeighbourUrn = 100287, Dist = 0.5100, Rank = 13, Ks2Rp = 109.80, Ks2Mp = 108.90, PpPerc = 15.20, PercentEal = 8.30, Polar4QuintilePupils = 5, PStability = 94.50, IdaciPupils = 0.135, PercentSchSupport = 9.80, NumberOfPupils = 1000, PercentStatementOrEhp = 0.85, Att8Scr = 62.4, EstablishmentName = "Thornton Grammar School", Street = "Oak Avenue", Town = "Bradford", County = "West Yorkshire", Postcode = "BD8 7HN", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Selective", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1000, OverallAbsenceRate = 3.2, PersistentAbsenceRate = 6.5 },
            new() { Urn = urn, NeighbourUrn = 100288, Dist = 0.5180, Rank = 14, Ks2Rp = 103.90, Ks2Mp = 103.20, PpPerc = 45.60, PercentEal = 52.30, Polar4QuintilePupils = 2, PStability = 85.40, IdaciPupils = 0.290, PercentSchSupport = 17.50, NumberOfPupils = 650, PercentStatementOrEhp = 3.10, Att8Scr = 41.2, EstablishmentName = "Riverside Academy", Street = "Ferry Road", Town = "Norwich", County = "Norfolk", Postcode = "NR1 5QS", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Free school", Region = "East of England", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 650, OverallAbsenceRate = 6.3, PersistentAbsenceRate = 14.8 },
            new() { Urn = urn, NeighbourUrn = 100289, Dist = 0.5250, Rank = 15, Ks2Rp = 105.60, Ks2Mp = 104.70, PpPerc = 34.80, PercentEal = 5.20, Polar4QuintilePupils = 3, PStability = 88.90, IdaciPupils = 0.210, PercentSchSupport = 13.60, NumberOfPupils = 500, PercentStatementOrEhp = 2.00, Att8Scr = 48.3, EstablishmentName = "Lakeside High School", Street = "Waterfront Close", Town = "Windermere", County = "Cumbria", Postcode = "LA23 1BX", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "North West", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 500, OverallAbsenceRate = 4.9, PersistentAbsenceRate = 11.2 },
            new() { Urn = urn, NeighbourUrn = 100290, Dist = 0.5340, Rank = 16, Ks2Rp = 104.20, Ks2Mp = 103.80, PpPerc = 40.10, PercentEal = 35.60, Polar4QuintilePupils = 3, PStability = 87.20, IdaciPupils = 0.248, PercentSchSupport = 16.20, NumberOfPupils = 720, PercentStatementOrEhp = 2.60, Att8Scr = 45.1, EstablishmentName = "Kingsley Free School", Street = "Crown Street", Town = "Nottingham", County = "Nottinghamshire", Postcode = "NG1 6FJ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Free school", Region = "East Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Non-selective", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 720, OverallAbsenceRate = 5.7, PersistentAbsenceRate = 13.0 },
            new() { Urn = urn, NeighbourUrn = 100291, Dist = 0.5430, Rank = 17, Ks2Rp = 107.80, Ks2Mp = 106.80, PpPerc = 24.30, PercentEal = 15.40, Polar4QuintilePupils = 4, PStability = 91.80, IdaciPupils = 0.178, PercentSchSupport = 11.80, NumberOfPupils = 950, PercentStatementOrEhp = 1.25, Att8Scr = 52.7, EstablishmentName = "Beacon Hill School", Street = "Hilltop Lane", Town = "Bath", County = "Somerset", Postcode = "BA1 5RG", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 950, OverallAbsenceRate = 4.1, PersistentAbsenceRate = 9.4 },
            new() { Urn = urn, NeighbourUrn = 100292, Dist = 0.5510, Rank = 18, Ks2Rp = 106.50, Ks2Mp = 105.60, PpPerc = 29.80, PercentEal = 20.10, Polar4QuintilePupils = 3, PStability = 89.50, IdaciPupils = 0.205, PercentSchSupport = 14.00, NumberOfPupils = 800, PercentStatementOrEhp = 1.65, Att8Scr = 49.8, EstablishmentName = "Westgate Boys' School", Street = "Gate Road", Town = "Canterbury", County = "Kent", Postcode = "CT1 2RX", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Voluntary controlled school", Region = "South East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Boys", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 800, OverallAbsenceRate = 5.0, PersistentAbsenceRate = 11.5 },
            new() { Urn = urn, NeighbourUrn = 100293, Dist = 0.5600, Rank = 19, Ks2Rp = 103.50, Ks2Mp = 102.80, PpPerc = 48.20, PercentEal = 38.50, Polar4QuintilePupils = 2, PStability = 84.60, IdaciPupils = 0.305, PercentSchSupport = 18.30, NumberOfPupils = 680, PercentStatementOrEhp = 3.40, Att8Scr = 40.5, EstablishmentName = "Hartfield Secondary School", Street = "Meadow Lane", Town = "Darlington", County = "County Durham", Postcode = "DL1 4TH", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "SEN unit", SchoolCapacity = 680, OverallAbsenceRate = 6.5, PersistentAbsenceRate = 15.2 },
            new() { Urn = urn, NeighbourUrn = 100294, Dist = 0.5680, Rank = 20, Ks2Rp = 105.90, Ks2Mp = 104.50, PpPerc = 36.50, PercentEal = 11.20, Polar4QuintilePupils = 3, PStability = 88.70, IdaciPupils = 0.225, PercentSchSupport = 15.30, NumberOfPupils = 900, PercentStatementOrEhp = 2.15, Att8Scr = 44.2, EstablishmentName = "Pendle View High School", Street = "Valley Road", Town = "Burnley", County = "Lancashire", Postcode = "BB11 3QE", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "North West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 900, OverallAbsenceRate = 5.9, PersistentAbsenceRate = 13.8 },

            // Rank 21-30
            new() { Urn = urn, NeighbourUrn = 100295, Dist = 0.5750, Rank = 21, Ks2Rp = 107.20, Ks2Mp = 106.40, PpPerc = 26.80, PercentEal = 22.50, Polar4QuintilePupils = 4, PStability = 90.90, IdaciPupils = 0.192, PercentSchSupport = 12.50, NumberOfPupils = 1100, PercentStatementOrEhp = 1.40, Att8Scr = 50.3, EstablishmentName = "Millfield Academy", Street = "Mill Lane", Town = "Swindon", County = "Wiltshire", Postcode = "SN1 4AD", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1100, OverallAbsenceRate = 4.3, PersistentAbsenceRate = 9.6 },
            new() { Urn = urn, NeighbourUrn = 100296, Dist = 0.5830, Rank = 22, Ks2Rp = 103.20, Ks2Mp = 102.50, PpPerc = 50.30, PercentEal = 42.10, Polar4QuintilePupils = 2, PStability = 83.80, IdaciPupils = 0.320, PercentSchSupport = 19.10, NumberOfPupils = 750, PercentStatementOrEhp = 3.60, Att8Scr = 38.4, EstablishmentName = "Ashfield Comprehensive", Street = "Park Road", Town = "Mansfield", County = "Nottinghamshire", Postcode = "NG18 2AB", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "East Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 750, OverallAbsenceRate = 7.1, PersistentAbsenceRate = 16.4 },
            new() { Urn = urn, NeighbourUrn = 100297, Dist = 0.5910, Rank = 23, Ks2Rp = 108.30, Ks2Mp = 107.50, PpPerc = 21.50, PercentEal = 16.80, Polar4QuintilePupils = 4, PStability = 92.10, IdaciPupils = 0.175, PercentSchSupport = 10.90, NumberOfPupils = 1000, PercentStatementOrEhp = 1.15, Att8Scr = 54.8, EstablishmentName = "Cromwell High School", Street = "Station Road", Town = "Huntingdon", County = "Cambridgeshire", Postcode = "PE29 3QG", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East of England", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1000, OverallAbsenceRate = 4.0, PersistentAbsenceRate = 8.7 },
            new() { Urn = urn, NeighbourUrn = 100298, Dist = 0.5990, Rank = 24, Ks2Rp = 109.10, Ks2Mp = 108.20, PpPerc = 17.90, PercentEal = 24.30, Polar4QuintilePupils = 5, PStability = 93.60, IdaciPupils = 0.158, PercentSchSupport = 10.00, NumberOfPupils = 1300, PercentStatementOrEhp = 0.92, Att8Scr = 56.1, EstablishmentName = "Silverdale School", Street = "Silver Lane", Town = "Sheffield", County = "South Yorkshire", Postcode = "S11 9QH", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1300, OverallAbsenceRate = 3.8, PersistentAbsenceRate = 8.0 },
            new() { Urn = urn, NeighbourUrn = 100299, Dist = 0.6070, Rank = 25, Ks2Rp = 104.60, Ks2Mp = 103.90, PpPerc = 39.20, PercentEal = 33.40, Polar4QuintilePupils = 3, PStability = 87.50, IdaciPupils = 0.242, PercentSchSupport = 15.80, NumberOfPupils = 820, PercentStatementOrEhp = 2.45, Att8Scr = 43.7, EstablishmentName = "Holgate Academy", Street = "Holgate Lane", Town = "Hucknall", County = "Nottinghamshire", Postcode = "NG15 7LT", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 820, OverallAbsenceRate = 5.6, PersistentAbsenceRate = 12.9 },
            new() { Urn = urn, NeighbourUrn = 100300, Dist = 0.6150, Rank = 26, Ks2Rp = 107.00, Ks2Mp = 106.20, PpPerc = 27.50, PercentEal = 18.60, Polar4QuintilePupils = 4, PStability = 90.50, IdaciPupils = 0.195, PercentSchSupport = 13.20, NumberOfPupils = 1150, PercentStatementOrEhp = 1.50, Att8Scr = 48.9, EstablishmentName = "Priory School", Street = "Priory Road", Town = "Lewes", County = "East Sussex", Postcode = "BN7 1HE", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1150, OverallAbsenceRate = 4.4, PersistentAbsenceRate = 10.1 },
            new() { Urn = urn, NeighbourUrn = 100301, Dist = 0.6230, Rank = 27, Ks2Rp = 108.00, Ks2Mp = 107.10, PpPerc = 23.80, PercentEal = 14.20, Polar4QuintilePupils = 4, PStability = 91.40, IdaciPupils = 0.182, PercentSchSupport = 11.60, NumberOfPupils = 1400, PercentStatementOrEhp = 1.22, Att8Scr = 52.3, EstablishmentName = "Collingwood School", Street = "Kingston Road", Town = "Camberley", County = "Surrey", Postcode = "GU15 4AE", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Foundation school", Region = "South East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1400, OverallAbsenceRate = 3.9, PersistentAbsenceRate = 8.5 },
            new() { Urn = urn, NeighbourUrn = 100302, Dist = 0.6310, Rank = 28, Ks2Rp = 105.40, Ks2Mp = 104.60, PpPerc = 35.90, PercentEal = 7.80, Polar4QuintilePupils = 3, PStability = 88.60, IdaciPupils = 0.218, PercentSchSupport = 14.80, NumberOfPupils = 900, PercentStatementOrEhp = 1.95, Att8Scr = 46.5, EstablishmentName = "Broughton High School", Street = "Broughton Lane", Town = "Preston", County = "Lancashire", Postcode = "PR3 5JA", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "North West", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 900, OverallAbsenceRate = 5.1, PersistentAbsenceRate = 11.8 },
            new() { Urn = urn, NeighbourUrn = 100303, Dist = 0.6390, Rank = 29, Ks2Rp = 108.80, Ks2Mp = 107.90, PpPerc = 19.20, PercentEal = 21.80, Polar4QuintilePupils = 5, PStability = 93.20, IdaciPupils = 0.162, PercentSchSupport = 10.40, NumberOfPupils = 1200, PercentStatementOrEhp = 1.05, Att8Scr = 58.2, EstablishmentName = "Arden Academy", Street = "Station Road", Town = "Solihull", County = "West Midlands", Postcode = "B93 0PT", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "West Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1200, OverallAbsenceRate = 3.5, PersistentAbsenceRate = 7.6 },
            new() { Urn = urn, NeighbourUrn = 100304, Dist = 0.6470, Rank = 30, Ks2Rp = 104.10, Ks2Mp = 103.40, PpPerc = 43.50, PercentEal = 3.60, Polar4QuintilePupils = 2, PStability = 86.30, IdaciPupils = 0.255, PercentSchSupport = 16.50, NumberOfPupils = 550, PercentStatementOrEhp = 2.70, Att8Scr = 41.8, EstablishmentName = "Whitby High School", Street = "Seaview Terrace", Town = "Whitby", County = "North Yorkshire", Postcode = "YO21 3EG", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Community school", Region = "Yorkshire and The Humber", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 550, OverallAbsenceRate = 6.0, PersistentAbsenceRate = 13.9 },

            // Rank 31-40
            new() { Urn = urn, NeighbourUrn = 100305, Dist = 0.6550, Rank = 31, Ks2Rp = 110.20, Ks2Mp = 109.50, PpPerc = 12.80, PercentEal = 6.50, Polar4QuintilePupils = 5, PStability = 95.10, IdaciPupils = 0.120, PercentSchSupport = 8.90, NumberOfPupils = 700, PercentStatementOrEhp = 0.75, Att8Scr = 60.5, EstablishmentName = "Kingswood School", Street = "Lansdown Road", Town = "Bath", County = "Somerset", Postcode = "BA1 5RJ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South West", UrbanOrRural = "Urban", AdmissionsPolicy = "Selective", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 700, OverallAbsenceRate = 3.1, PersistentAbsenceRate = 6.8 },
            new() { Urn = urn, NeighbourUrn = 100306, Dist = 0.6630, Rank = 32, Ks2Rp = 102.80, Ks2Mp = 101.90, PpPerc = 52.60, PercentEal = 2.10, Polar4QuintilePupils = 1, PStability = 82.50, IdaciPupils = 0.340, PercentSchSupport = 20.50, NumberOfPupils = 400, PercentStatementOrEhp = 3.80, Att8Scr = 37.1, EstablishmentName = "Haydon Bridge High School", Street = "Tyne Road", Town = "Haydon Bridge", County = "Northumberland", Postcode = "NE47 6LR", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North East", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 400, OverallAbsenceRate = 7.2, PersistentAbsenceRate = 16.8 },
            new() { Urn = urn, NeighbourUrn = 100307, Dist = 0.6710, Rank = 33, Ks2Rp = 108.60, Ks2Mp = 107.70, PpPerc = 20.40, PercentEal = 27.50, Polar4QuintilePupils = 4, PStability = 92.50, IdaciPupils = 0.172, PercentSchSupport = 11.10, NumberOfPupils = 1500, PercentStatementOrEhp = 1.08, Att8Scr = 55.9, EstablishmentName = "Tapton School", Street = "Darwin Lane", Town = "Sheffield", County = "South Yorkshire", Postcode = "S10 5RG", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1500, OverallAbsenceRate = 3.6, PersistentAbsenceRate = 7.8 },
            new() { Urn = urn, NeighbourUrn = 100308, Dist = 0.6790, Rank = 34, Ks2Rp = 106.70, Ks2Mp = 105.80, PpPerc = 28.90, PercentEal = 9.40, Polar4QuintilePupils = 3, PStability = 89.90, IdaciPupils = 0.202, PercentSchSupport = 13.40, NumberOfPupils = 850, PercentStatementOrEhp = 1.72, Att8Scr = 49.1, EstablishmentName = "Queen Elizabeth School", Street = "Queen Street", Town = "Crediton", County = "Devon", Postcode = "EX17 3LR", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South West", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 850, OverallAbsenceRate = 4.7, PersistentAbsenceRate = 10.8 },
            new() { Urn = urn, NeighbourUrn = 100309, Dist = 0.6870, Rank = 35, Ks2Rp = 104.40, Ks2Mp = 103.60, PpPerc = 41.20, PercentEal = 40.80, Polar4QuintilePupils = 2, PStability = 86.10, IdaciPupils = 0.268, PercentSchSupport = 16.90, NumberOfPupils = 1050, PercentStatementOrEhp = 2.55, Att8Scr = 43.4, EstablishmentName = "Outwood Academy", Street = "Ledger Lane", Town = "Wakefield", County = "West Yorkshire", Postcode = "WF1 2PH", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1050, OverallAbsenceRate = 5.8, PersistentAbsenceRate = 13.2 },
            new() { Urn = urn, NeighbourUrn = 100310, Dist = 0.6950, Rank = 36, Ks2Rp = 106.80, Ks2Mp = 106.00, PpPerc = 27.10, PercentEal = 19.20, Polar4QuintilePupils = 4, PStability = 90.30, IdaciPupils = 0.198, PercentSchSupport = 13.00, NumberOfPupils = 1250, PercentStatementOrEhp = 1.48, Att8Scr = 47.6, EstablishmentName = "Wolfreton School", Street = "Main Road", Town = "Hull", County = "East Yorkshire", Postcode = "HU10 7LU", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1250, OverallAbsenceRate = 5.0, PersistentAbsenceRate = 11.4 },
            new() { Urn = urn, NeighbourUrn = 100311, Dist = 0.7030, Rank = 37, Ks2Rp = 103.70, Ks2Mp = 102.90, PpPerc = 44.80, PercentEal = 48.60, Polar4QuintilePupils = 2, PStability = 84.90, IdaciPupils = 0.295, PercentSchSupport = 17.80, NumberOfPupils = 900, PercentStatementOrEhp = 3.20, Att8Scr = 40.8, EstablishmentName = "Fernwood School", Street = "Goodwood Road", Town = "Nottingham", County = "Nottinghamshire", Postcode = "NG3 6AP", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 900, OverallAbsenceRate = 6.4, PersistentAbsenceRate = 14.5 },
            new() { Urn = urn, NeighbourUrn = 100312, Dist = 0.7110, Rank = 38, Ks2Rp = 103.00, Ks2Mp = 102.20, PpPerc = 51.40, PercentEal = 36.90, Polar4QuintilePupils = 2, PStability = 83.20, IdaciPupils = 0.330, PercentSchSupport = 19.60, NumberOfPupils = 700, PercentStatementOrEhp = 3.50, Att8Scr = 38.9, EstablishmentName = "The Blyth Academy", Street = "Chase Farm Drive", Town = "Blyth", County = "Northumberland", Postcode = "NE24 4LP", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 700, OverallAbsenceRate = 7.0, PersistentAbsenceRate = 16.1 },
            new() { Urn = urn, NeighbourUrn = 100313, Dist = 0.7190, Rank = 39, Ks2Rp = 105.50, Ks2Mp = 104.40, PpPerc = 37.30, PercentEal = 26.80, Polar4QuintilePupils = 3, PStability = 87.80, IdaciPupils = 0.235, PercentSchSupport = 15.40, NumberOfPupils = 800, PercentStatementOrEhp = 2.35, Att8Scr = 46.2, EstablishmentName = "Patcham High School", Street = "Ladies Mile Road", Town = "Brighton", County = "East Sussex", Postcode = "BN1 8PB", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "Resourced provision", SchoolCapacity = 800, OverallAbsenceRate = 5.3, PersistentAbsenceRate = 12.3 },
            new() { Urn = urn, NeighbourUrn = 100314, Dist = 0.7270, Rank = 40, Ks2Rp = 106.20, Ks2Mp = 105.20, PpPerc = 32.40, PercentEal = 13.50, Polar4QuintilePupils = 3, PStability = 89.20, IdaciPupils = 0.212, PercentSchSupport = 14.30, NumberOfPupils = 2000, PercentStatementOrEhp = 1.82, Att8Scr = 50.6, EstablishmentName = "Ivybridge Community College", Street = "Harford Road", Town = "Ivybridge", County = "Devon", Postcode = "PL21 0JA", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "South West", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 2000, OverallAbsenceRate = 4.5, PersistentAbsenceRate = 10.2 },

            // Rank 41-50
            new() { Urn = urn, NeighbourUrn = 100315, Dist = 0.7350, Rank = 41, Ks2Rp = 105.70, Ks2Mp = 104.80, PpPerc = 34.20, PercentEal = 6.80, Polar4QuintilePupils = 3, PStability = 88.40, IdaciPupils = 0.222, PercentSchSupport = 14.60, NumberOfPupils = 500, PercentStatementOrEhp = 2.05, Att8Scr = 45.7, EstablishmentName = "Settle College", Street = "Duke Street", Town = "Settle", County = "North Yorkshire", Postcode = "BD24 9AA", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 500, OverallAbsenceRate = 4.8, PersistentAbsenceRate = 10.9 },
            new() { Urn = urn, NeighbourUrn = 100316, Dist = 0.7430, Rank = 42, Ks2Rp = 104.90, Ks2Mp = 103.70, PpPerc = 38.10, PercentEal = 17.40, Polar4QuintilePupils = 3, PStability = 87.60, IdaciPupils = 0.240, PercentSchSupport = 15.90, NumberOfPupils = 750, PercentStatementOrEhp = 2.40, Att8Scr = 42.3, EstablishmentName = "Netherthorpe School", Street = "Market Street", Town = "Staveley", County = "Derbyshire", Postcode = "S43 3TW", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Foundation school", Region = "East Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 750, OverallAbsenceRate = 5.9, PersistentAbsenceRate = 13.6 },
            new() { Urn = urn, NeighbourUrn = 100317, Dist = 0.7510, Rank = 43, Ks2Rp = 107.40, Ks2Mp = 106.60, PpPerc = 25.30, PercentEal = 11.90, Polar4QuintilePupils = 4, PStability = 91.20, IdaciPupils = 0.185, PercentSchSupport = 12.30, NumberOfPupils = 1100, PercentStatementOrEhp = 1.32, Att8Scr = 51.5, EstablishmentName = "Sponne School", Street = "Brackley Road", Town = "Towcester", County = "Northamptonshire", Postcode = "NN12 6DJ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East Midlands", UrbanOrRural = "Rural", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1100, OverallAbsenceRate = 4.2, PersistentAbsenceRate = 9.3 },
            new() { Urn = urn, NeighbourUrn = 100318, Dist = 0.7590, Rank = 44, Ks2Rp = 106.40, Ks2Mp = 105.50, PpPerc = 30.60, PercentEal = 23.70, Polar4QuintilePupils = 3, PStability = 89.70, IdaciPupils = 0.208, PercentSchSupport = 13.70, NumberOfPupils = 1350, PercentStatementOrEhp = 1.58, Att8Scr = 48.4, EstablishmentName = "Bridgewater High School", Street = "Broomfields Road", Town = "Warrington", County = "Cheshire", Postcode = "WA4 6RD", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1350, OverallAbsenceRate = 4.6, PersistentAbsenceRate = 10.4 },
            new() { Urn = urn, NeighbourUrn = 100319, Dist = 0.7670, Rank = 45, Ks2Rp = 110.50, Ks2Mp = 109.80, PpPerc = 11.50, PercentEal = 4.20, Polar4QuintilePupils = 5, PStability = 95.80, IdaciPupils = 0.108, PercentSchSupport = 8.40, NumberOfPupils = 850, PercentStatementOrEhp = 0.70, Att8Scr = 61.8, EstablishmentName = "Carre's Grammar School", Street = "Northgate", Town = "Sleaford", County = "Lincolnshire", Postcode = "NG34 7DD", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East Midlands", UrbanOrRural = "Rural", AdmissionsPolicy = "Selective", Gender = "Boys", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 850, OverallAbsenceRate = 3.3, PersistentAbsenceRate = 7.0 },
            new() { Urn = urn, NeighbourUrn = 100320, Dist = 0.7750, Rank = 46, Ks2Rp = 103.40, Ks2Mp = 102.60, PpPerc = 49.50, PercentEal = 44.30, Polar4QuintilePupils = 2, PStability = 84.10, IdaciPupils = 0.310, PercentSchSupport = 18.80, NumberOfPupils = 800, PercentStatementOrEhp = 3.30, Att8Scr = 39.5, EstablishmentName = "Barnwell School", Street = "Barnwell Gate", Town = "Stevenage", County = "Hertfordshire", Postcode = "SG2 9XH", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "East of England", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 800, OverallAbsenceRate = 6.7, PersistentAbsenceRate = 15.5 },
            new() { Urn = urn, NeighbourUrn = 100321, Dist = 0.7830, Rank = 47, Ks2Rp = 107.90, Ks2Mp = 107.00, PpPerc = 22.60, PercentEal = 16.10, Polar4QuintilePupils = 4, PStability = 91.70, IdaciPupils = 0.180, PercentSchSupport = 11.90, NumberOfPupils = 1000, PercentStatementOrEhp = 1.28, Att8Scr = 53.4, EstablishmentName = "Parkside School", Street = "College Road", Town = "Bromsgrove", County = "Worcestershire", Postcode = "B61 8RE", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "West Midlands", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 1000, OverallAbsenceRate = 4.1, PersistentAbsenceRate = 9.0 },
            new() { Urn = urn, NeighbourUrn = 100322, Dist = 0.7910, Rank = 48, Ks2Rp = 102.50, Ks2Mp = 101.60, PpPerc = 55.20, PercentEal = 28.40, Polar4QuintilePupils = 1, PStability = 81.80, IdaciPupils = 0.360, PercentSchSupport = 21.20, NumberOfPupils = 700, PercentStatementOrEhp = 4.10, Att8Scr = 36.7, EstablishmentName = "Whitehaven Academy", Street = "Flatt Walks", Town = "Whitehaven", County = "Cumbria", Postcode = "CA28 7RJ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North West", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "SEN unit", SchoolCapacity = 700, OverallAbsenceRate = 7.5, PersistentAbsenceRate = 17.2 },
            new() { Urn = urn, NeighbourUrn = 100323, Dist = 0.7990, Rank = 49, Ks2Rp = 104.30, Ks2Mp = 103.30, PpPerc = 42.80, PercentEal = 31.50, Polar4QuintilePupils = 2, PStability = 85.70, IdaciPupils = 0.275, PercentSchSupport = 17.10, NumberOfPupils = 750, PercentStatementOrEhp = 2.85, Att8Scr = 41.3, EstablishmentName = "Cleethorpes Academy", Street = "Grainsby Avenue", Town = "Cleethorpes", County = "Lincolnshire", Postcode = "DN35 7LQ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "Yorkshire and The Humber", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = false, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 750, OverallAbsenceRate = 6.2, PersistentAbsenceRate = 14.0 },
            new() { Urn = urn, NeighbourUrn = 100324, Dist = 0.8070, Rank = 50, Ks2Rp = 105.30, Ks2Mp = 104.30, PpPerc = 36.70, PercentEal = 14.80, Polar4QuintilePupils = 3, PStability = 88.10, IdaciPupils = 0.228, PercentSchSupport = 15.20, NumberOfPupils = 650, PercentStatementOrEhp = 2.18, Att8Scr = 44.9, EstablishmentName = "Prudhoe Community High School", Street = "Moor Road", Town = "Prudhoe", County = "Northumberland", Postcode = "NE42 5LJ", PhaseOfEducation = "Secondary", TypeOfEstablishment = "Academy", Region = "North East", UrbanOrRural = "Urban", AdmissionsPolicy = "Comprehensive", Gender = "Mixed", HasSixthForm = true, HasNurseryProvision = false, ResourcedProvisionType = "None", SchoolCapacity = 650, OverallAbsenceRate = 5.5, PersistentAbsenceRate = 12.6 },
        };
    }
}