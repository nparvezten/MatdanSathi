using System;
using MatdanSathi.API.Domain.Common;

namespace MatdanSathi.API.Domain.Entities;

public class BloCoordinateMap : BaseEntity
{
    public string BloName { get; set; } = null!;
    public string BloContactEncrypted { get; set; } = null!;
    public string PollingStationName { get; set; } = null!;

    // Geographical center point
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // GeoJSON or spatial coordinate string representing boundary polygons
    public string BoundaryCoordinatesJson { get; set; } = null!;
}
