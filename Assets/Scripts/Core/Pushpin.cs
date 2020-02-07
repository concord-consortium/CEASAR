using System;
using UnityEngine;

[Serializable]
public class Pushpin
{
    public LatLng Location;
    public DateTime SelectedDateTime;
    public string LocationName;
    [SerializeField] private string ReadableDate;
    
    public override string ToString()
    {
        return Location.ToString() + " " + SelectedDateTime.ToString();
    }

    public Pushpin()
    {
        this.SelectedDateTime = DateTime.UtcNow;
        this.Location = new LatLng();
        this.LocationName = SimulationConstants.CUSTOM_LOCATION;
    }

    public Pushpin(DateTime dt, LatLng latLng, string locationName)
    {
        this.SelectedDateTime = dt;
        this.Location = latLng;
        this.LocationName = String.IsNullOrEmpty(locationName) ? SimulationConstants.CUSTOM_LOCATION : locationName;
        this.ReadableDate = SelectedDateTime.ToString();
    }
    
    public override bool Equals(System.Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || ! this.GetType().Equals(obj.GetType())) 
        {
            return false;
        }
        else { 
            Pushpin p = (Pushpin) obj; 
            return (Location == p.Location) && (SelectedDateTime == p.SelectedDateTime);
        }   
    }

    public override int GetHashCode()
    { 
        int t = this.SelectedDateTime.Hour + this.SelectedDateTime.Minute + this.SelectedDateTime.Second;
        int ll = (int)this.Location.Latitude + (int)this.Location.Longitude;
        return (t + ll);
    }
}

[Serializable]
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