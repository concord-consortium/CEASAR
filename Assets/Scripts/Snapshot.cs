using System;

public class Snapshot
{
    public DateTime dateTime;
    public string location;
    public LatLng locationCoordinates;
    public Snapshot(DateTime dt, string loc, LatLng locCoords)
    {
        dateTime = dt;
        location = loc;
        locationCoordinates = locCoords;
    }
}
