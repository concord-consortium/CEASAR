using System;
using UnityEngine;

public struct AltAz
{
    public float Altitude;
    public float Azimuth;
    public override string ToString()
    {
        return "" + Altitude + "," + Azimuth;
    }
}
public static class Utils
{
    public static void SetObjectColor(GameObject go, Color newColor)
    {
        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        if (mesh)
        {
            Vector3[] vertices = mesh.vertices;

            // create new colors array where the colors will be created.
            Color[] colors = new Color[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                colors[i] = newColor;

            // assign the array of colors to the Mesh.
            mesh.colors = colors;
        }
    }
    public static bool CompareNetworkTransform(NetworkTransform oldT, NetworkTransform newT)
    {
        if ((oldT.position.x != newT.position.x) ||
               (oldT.position.y != newT.position.y) ||
                (oldT.position.z != newT.position.z) ||
                 (oldT.rotation.x != newT.rotation.x) ||
                  (oldT.rotation.y != newT.rotation.y) ||
                   (oldT.rotation.z != newT.rotation.z)) return true;
        return false;
    }
    public static Vector3 NetworkV3ToVector3(NetworkVector3 pos)
    {
        return new Vector3(pos.x, pos.y, pos.z);
    }
    public static Quaternion NetworkV3ToQuaternion(NetworkVector3 rot)
    {
        return Quaternion.Euler(rot.x, rot.y, rot.z);
    }
    // inputs: localSiderialTime is in decimal hours and alpha is right ascension of the object of interest in decimal hours
    private static float hourAngle(double localSiderialTime, float alpha)
    {
        float HA = (float)localSiderialTime - alpha;
        if (HA < 0)
        {
            HA = HA + 24; // if hour angle is negative add 24 hours
        }
        return HA; //hour angle in decimal hours
    }
    // functions to transform equitorial coordinates(RA, Dec) to Cartesian(x, y, z) for the celestial sphere for plotting in the 3D space.
    public static Vector3 CalculateEquitorialPosition(float radianRA, float radianDec, float radius)
    {
        var xPos = radius * (Mathf.Cos(radianRA) * Mathf.Cos(radianDec));
        var zPos = radius * (Mathf.Sin(radianRA)) * Mathf.Cos(radianDec);
        var yPos = radius * (Mathf.Sin(radianDec));
        return new Vector3(xPos, yPos, zPos);
    }

    public static AltAz CalculateAltitudeAzimuthForStar(float RA, float Dec,
        double currentSiderialTime, float observerLatitude)
    {
        float radianDec = Dec * Mathf.Deg2Rad;
        float Alt = 0;
        float Azm = 0;
        // convert all things to Radians for Unity
        float radianLatitude = Mathf.Deg2Rad * observerLatitude;

        // convert from RA/Dec to Altitude/Azimuth (north = 0)
        float h = hourAngle(currentSiderialTime, RA);
        float radianHDegreesPerHour = h * 15 * Mathf.Deg2Rad;

        float sinAltitude = (Mathf.Sin(radianDec) * Mathf.Sin(radianLatitude)) + (Mathf.Cos(radianDec) * Mathf.Cos(radianLatitude) * Mathf.Cos(radianHDegreesPerHour));
        float altitude = Mathf.Asin(sinAltitude);

        float cosAzimuth = (Mathf.Sin(radianDec) - (Mathf.Sin(radianLatitude) * sinAltitude)) / (Mathf.Cos(radianLatitude) * Mathf.Cos(altitude));
        float azimuthRaw = Mathf.Acos(cosAzimuth);

        float sinH = Mathf.Sin(radianHDegreesPerHour);
        if (sinH < 0)
        {
            Azm = azimuthRaw;
        }
        else
        {
            Azm = (Mathf.Deg2Rad * 360) - azimuthRaw;
        }
        Alt = altitude; // should now be in radians

        if (float.IsNaN(Alt)) Alt = 0;
        if (float.IsNaN(Azm)) Azm = 0;
        return new AltAz {Altitude = Mathf.Rad2Deg * Alt, Azimuth = Mathf.Rad2Deg * Azm};
    }
    [Obsolete]
    public static Vector3 CalculateHorizonPosition(float RA, float Dec, float radius, double currentSiderialTime, float observerLatitude)
    {
        AltAz altaz = CalculateAltitudeAzimuthForStar( RA, Dec, currentSiderialTime,  observerLatitude);
        return CalculatePositionByAzAlt(altaz.Azimuth * Mathf.Deg2Rad, altaz.Altitude * Mathf.Deg2Rad, radius);
    }

    public static Vector3 CalculatePositionByAzAlt(float azimuth, float altitude, float radius)
    {
        // our ground uses Z for North, so we need to flip this around a little
        var zPos = radius * (Mathf.Cos(azimuth)) * (Mathf.Cos(altitude)) * -1; 
        var xPos = radius * (Mathf.Cos(altitude) * (Mathf.Sin(azimuth))) * -1;
        var yPos = radius * Mathf.Sin(altitude);

        if (float.IsNaN(xPos))
        {
            CCConsoleLog.Log($"{azimuth} {altitude}");
        }
        return new Vector3(xPos, yPos, zPos);
    }
    public static Vector3 CalculatePositionByAzAlt(double azimuth, double altitude, float radius)
    {
        return CalculatePositionByAzAlt((float)azimuth, (float)altitude, radius);
    }
    public static LatLng LatLngFromPosition(Vector3 pos, float rad)
    {
        float lat = (float) Math.Asin(pos.y / rad) * 180 / Mathf.PI;
        float lng = ((float) Math.Atan(pos.x / pos.z) * -180 / Mathf.PI ) - 90;
        if (pos.z > 0) lng += 180;

        LatLng latlng = new LatLng();
        latlng.Latitude = lat;
        latlng.Longitude = lng;
        return latlng;
    }

    public static Vector3 PositionFromLatLng(LatLng latlng, float radius)
    {
        var lat = latlng.Latitude * Mathf.Deg2Rad;
        var lng = latlng.Longitude * Mathf.Deg2Rad;
        // y is up
        var x = radius * Mathf.Cos(lat) * Mathf.Cos(lng);
        var y = radius * Mathf.Sin(lat);
        var z = radius * Mathf.Cos(lat) * Mathf.Sin(lng);
        
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static Color GetColorFromTexture(Renderer rend, Vector2 pixelUV)
    {
        if (rend == null)
        {
            CCConsoleLog.Log("no renderer");
            return Color.black;
        }
        Texture2D tex = rend.material.GetTexture("_MainTex") as Texture2D;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;
        // apply our offset on X axis
        pixelUV.x += (rend.material.GetTextureOffset("_MainTex").x * tex.width);

        Color pixelColor = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        CCConsoleLog.Log(pixelColor);
        return pixelColor;
    }

    // Given a compass angle in degrees rotation, return Ordinal Name.
    public static string CalcCompassOrdinal(float CompassDeg)
    {
        // Return the Group - Scene - Platform as a String.
        // 45 degree quadrants offset by ½ 45 or 22.5d
        float angle = 22.5f;
        float step = 45;
        int index = 0;
        string[] ordinals = {
            "North", "North East", "East", "South East",
            "South", "South West", "West", "North West"
        };
        while (angle <= 337.5)
        {
            if (CompassDeg < angle) return ordinals[index];
            index++;
            angle += step;
        }
        // if the viewAngle is > 337.5 its north:
        return ordinals[0];
    }

}


public static class StringExtensions
{
    public static string FirstCharToUpper(this string input)
    {
        switch (input)
        {
            case null: throw new ArgumentNullException(nameof(input));
            case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            default:
                char[] a = input.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
        }
    }
}

