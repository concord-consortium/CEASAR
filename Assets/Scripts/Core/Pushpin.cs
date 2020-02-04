using System;
using UnityEngine;

public class Pushpin
{
    public LatLng Location;
    public DateTime SelectedDateTime;
    public string LocationName;
    
    public override string ToString()
    {
        return Location.ToString() + " " + SelectedDateTime.ToString();
    }

    public Pushpin()
    {
        SelectedDateTime = DateTime.UtcNow;
    }

    public Pushpin(DateTime dt, LatLng latLng, string locationName)
    {
        SelectedDateTime = dt;
        Location = latLng;
        LocationName = String.IsNullOrEmpty(locationName) ? SimulationConstants.CUSTOM_LOCATION : locationName;
    }
}

public struct LatLng
{
    public float Latitude;
    public float Longitude;
    public override string ToString()
    {
        return "" + Latitude + "," + Longitude;
    }

    public string ToDisplayString()
    {
        return "" + Latitude.ToString("F2") + "," + Longitude.ToString("F2");
    }
    
    public LatLng(string latlngString)
    {
        float defaultLatitude = 0;
        float defaultLongitude = 0;
        // In case the parse fails
        Latitude = defaultLatitude;
        Longitude = defaultLongitude;

        if (latlngString.Length > 0 && latlngString.Contains(","))
        {
            if (float.TryParse(latlngString.Split(',')[0], out defaultLatitude))
            {
                Latitude = defaultLatitude;
            }

            if (float.TryParse(latlngString.Split(',')[1], out defaultLongitude))
            {
                Longitude = defaultLongitude;
            }
        }
    }
    
    public static bool operator ==(LatLng l1, LatLng l2) 
    {
        return l1.Equals(l2);
    }

    public static bool operator !=(LatLng l1, LatLng l2) 
    {
        return !l1.Equals(l2);
    }
}