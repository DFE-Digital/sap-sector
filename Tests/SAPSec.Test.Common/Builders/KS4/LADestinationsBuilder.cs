using SAPSec.Data.Dto.KS4.Destinations;

namespace SAPSec.Test.Common.Builders.KS4;

public class LADestinationsBuilder(string laId)
{
    string AllDest_Tot_LA_Current_Pct = string.Empty;
    string AllDest_Tot_LA_Previous_Pct = string.Empty;
    string AllDest_Tot_LA_Previous2_Pct = string.Empty;
    string Education_Tot_LA_Current_Pct = string.Empty;
    string Education_Tot_LA_Previous_Pct = string.Empty;
    string Education_Tot_LA_Previous2_Pct = string.Empty;
    string Apprentice_Tot_LA_Current_Pct = string.Empty;
    string Apprentice_Tot_LA_Previous_Pct = string.Empty;
    string Apprentice_Tot_LA_Previous2_Pct = string.Empty;
    string Employment_Tot_LA_Current_Pct = string.Empty;
    string Employment_Tot_LA_Previous_Pct = string.Empty;
    string Employment_Tot_LA_Previous2_Pct = string.Empty;

    public LADestinationsBuilder WithAllDest(string current, string prev, string prev2)
    {
        AllDest_Tot_LA_Current_Pct = current;
        AllDest_Tot_LA_Previous_Pct = prev;
        AllDest_Tot_LA_Previous2_Pct = prev2;

        return this;
    }

    public LADestinationsBuilder WithEducation(string current, string prev, string prev2)
    {
        Education_Tot_LA_Current_Pct = current;
        Education_Tot_LA_Previous_Pct = prev;
        Education_Tot_LA_Previous2_Pct = prev2;

        return this;
    }

    public LADestinationsBuilder WithApprenticeships(string current, string prev, string prev2)
    {
        Apprentice_Tot_LA_Current_Pct = current;
        Apprentice_Tot_LA_Previous_Pct = prev;
        Apprentice_Tot_LA_Previous2_Pct = prev2;

        return this;
    }

    public LADestinationsBuilder WithEmployment(string current, string prev, string prev2)
    {
        Employment_Tot_LA_Current_Pct = current;
        Employment_Tot_LA_Previous_Pct = prev;
        Employment_Tot_LA_Previous2_Pct = prev2;

        return this;
    }

    public LADestinations Build() =>
        new()
        {
            Id = laId,
            AllDest_Tot_LA_Current_Pct = MeasureValue.Parse(AllDest_Tot_LA_Current_Pct),
            AllDest_Tot_LA_Previous_Pct = MeasureValue.Parse(AllDest_Tot_LA_Previous_Pct),
            AllDest_Tot_LA_Previous2_Pct = MeasureValue.Parse(AllDest_Tot_LA_Previous2_Pct),
            Education_Tot_LA_Current_Pct = MeasureValue.Parse(Education_Tot_LA_Current_Pct),
            Education_Tot_LA_Previous_Pct = MeasureValue.Parse(Education_Tot_LA_Previous_Pct),
            Education_Tot_LA_Previous2_Pct = MeasureValue.Parse(Education_Tot_LA_Previous2_Pct),
            Apprentice_Tot_LA_Current_Pct = MeasureValue.Parse(Apprentice_Tot_LA_Current_Pct),
            Apprentice_Tot_LA_Previous_Pct = MeasureValue.Parse(Apprentice_Tot_LA_Previous_Pct),
            Apprentice_Tot_LA_Previous2_Pct = MeasureValue.Parse(Apprentice_Tot_LA_Previous2_Pct),
            Employment_Tot_LA_Current_Pct = MeasureValue.Parse(Employment_Tot_LA_Current_Pct),
            Employment_Tot_LA_Previous_Pct = MeasureValue.Parse(Employment_Tot_LA_Previous_Pct),
            Employment_Tot_LA_Previous2_Pct = MeasureValue.Parse(Employment_Tot_LA_Previous2_Pct),
        };
}
