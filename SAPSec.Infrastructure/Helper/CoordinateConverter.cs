using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace SAPSec.Infrastructure.Helper;

public static class CoordinateConverter
{

    public static (double Latitude, double Longitude) EastingNorthingToLatLon(
        double easting,
        double northing)
    {
        var csFactory = new CoordinateSystemFactory();

        // EPSG:27700 – British National Grid (OSGB36)
        var britishNationalGrid = csFactory.CreateFromWkt(
            @"PROJCS[""OSGB 1936 / British National Grid"",
                GEOGCS[""OSGB 1936"",
                    DATUM[""OSGB_1936"",
                        SPHEROID[""Airy 1830"",6377563.396,299.3249646]],
                    PRIMEM[""Greenwich"",0],
                    UNIT[""degree"",0.0174532925199433]],
                PROJECTION[""Transverse_Mercator""],
                PARAMETER[""latitude_of_origin"",49],
                PARAMETER[""central_meridian"",-2],
                PARAMETER[""scale_factor"",0.9996012717],
                PARAMETER[""false_easting"",400000],
                PARAMETER[""false_northing"",-100000],
                UNIT[""metre"",1]]"
        );

        var wgs84 = GeographicCoordinateSystem.WGS84;

        var transformFactory = new CoordinateTransformationFactory();
        var transform = transformFactory.CreateFromCoordinateSystems(
            britishNationalGrid, wgs84);

        // Transform returns [Longitude, Latitude]
        var result = transform.MathTransform.Transform(
            new[] { easting, northing });

        return (Latitude: result[1], Longitude: result[0]);
    }
}