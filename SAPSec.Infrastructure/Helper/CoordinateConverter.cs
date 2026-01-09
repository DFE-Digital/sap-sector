using GeoUK.Coordinates;
using GeoUK.Ellipsoids;
using GeoUK.Projections;

namespace SAPSec.Infrastructure.Helper;

public static class CoordinateConverter
{
    public static (double Latitude, double Longitude) EastingNorthingToLatLon(
        double easting,
        double northing)
    {
        // 1) Easting/Northing -> Cartesian (OSGB36 / Airy1830, British National Grid)
        Cartesian osgbCartesian = GeoUK.Convert.ToCartesian(
            new Airy1830(),
            new BritishNationalGrid(),
            new EastingNorthing(easting, northing));

        // 2) Transform OSGB36 -> ETRS89 (GeoUK notes ETRS89 is effectively WGS84)
        Cartesian etrs89Cartesian = GeoUK.Transform.Osgb36ToEtrs89(osgbCartesian);

        // 3) Cartesian -> Latitude/Longitude (WGS84)
        LatitudeLongitude wgsLatLon = GeoUK.Convert.ToLatitudeLongitude(new Wgs84(), etrs89Cartesian);

        return (Latitude: wgsLatLon.Latitude, Longitude: wgsLatLon.Longitude);
    }
}