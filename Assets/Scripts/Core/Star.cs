using UnityEngine;
using System;

public class CelestialSphereItem
{
    public float RA; //right ascension in decimal hours
    public float Dec; //declanation in degrees, positive for northern, negative for southern, 0 to 90
    public float Alt; //Altitude
    public float Azm; //Azimuth

    protected float radianRA;
    protected float radianDec;
    public bool hidden;

    public CelestialSphereItem()
    {

    }

    public CelestialSphereItem(float RA, float Dec)
    {
        this.RA = RA;
        this.radianRA = RA * 15 * Mathf.Deg2Rad; // RA in hours, so multiply RA by 15 deg / hr
        this.Dec = Dec;
        this.radianDec = this.Dec * Mathf.Deg2Rad;
    }

    // inputs: localSiderialTime is in decimal hours and alpha is right ascension of the object of interest in decimal hours
    private float hourAngle(double localSiderialTime, float alpha)
    {
        float HA = (float)localSiderialTime - alpha;
        if (HA < 0)
        {
            HA = HA + 24; // if hour angle is negative add 24 hours
        }
        return HA; //hour angle in decimal hours
    }

    // functions to transform equitorial coordinates(RA, Dec) to Cartesian(x, y, z) for the celestial sphere for plotting in the 3D space.
    public Vector3 CalculateEquitorialPosition(float radius)
    {
        var xPos = radius * (Mathf.Cos(radianRA) * Mathf.Cos(radianDec));
        var zPos = radius * (Mathf.Sin(radianRA)) * Mathf.Cos(radianDec);
        var yPos = radius * (Mathf.Sin(radianDec));
        return new Vector3(xPos, yPos, zPos);
    }

    // calculate the position of stars at a point on Earth
    public Vector3 CalculateHorizonPosition(float radius, double currentSiderialTime, float observerLatitude)
    {
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
        var zPos = radius * (Mathf.Cos(Azm)) * (Mathf.Cos(Alt)); // ; RA in hours, so multiply RA by 15 deg / hr
        var xPos = radius * (Mathf.Cos(Alt) * (Mathf.Sin(Azm)));
        var yPos = radius * Mathf.Sin(Alt);

        if (float.IsNaN(xPos))
        {
            Debug.Log(Azm + " " + Alt);
        }
        return new Vector3(xPos, yPos, zPos);

    }
}

public class Marker : CelestialSphereItem
{
    public string Name;
    public Marker(string Name, float RA, float Dec)
    {
        this.Name = Name;
        this.RA = RA;
        this.radianRA = RA * 15 * Mathf.Deg2Rad; // RA in hours, so multiply RA by 15 deg / hr
        this.Dec = Dec;
        this.radianDec = this.Dec * Mathf.Deg2Rad;
    }
}

public class Star : CelestialSphereItem
{
    public int Hipparcos;
    public string Constellation; //univeral 3-character constellation abbrievation
    public string ProperName;

    public float Dist; //star distance in parsecs
    public float Mag; //apparent magnitude (smallest numbers, e.g., negative numbers, are the brightest)
    public float AbsMag;
    public string Spectrum;
    public float ColorIndex;

    // other properties for a star
    public float size;

    public string XBayerFlamsteed; // this is a sequence of characters that combines Bayer and Flamsteed star id
    public string BayerDesignation;
    public string FlamsteedDesignation;

    public Star(int Hip, string Constellation, string ProperName, string XBFlamsteed, string Flamsteed, string Bayer, float RA, float Dec, float Dist, float Mag, float AbsMag, string Spectrum, float ColorIndex)
    {
        this.Hipparcos = Hip;
        this.Constellation = Constellation;
        this.ProperName = ProperName;
        this.XBayerFlamsteed = XBFlamsteed;
        this.FlamsteedDesignation = Flamsteed.Length > 0 ? Flamsteed + " " +  Constellation : "";
        this.BayerDesignation = Bayer.Length > 0 ? Bayer + " " +  Constellation : "";
        this.RA = RA;
        this.radianRA = RA * 15 * Mathf.Deg2Rad; // RA in hours, so multiply RA by 15 deg / hr
        this.Dec = Dec;
        this.radianDec = this.Dec * Mathf.Deg2Rad;
        this.Dist = Dist;
        this.Mag = Mag;
        this.AbsMag = AbsMag;
        this.Spectrum = Spectrum;
        this.ColorIndex = ColorIndex;
    }
}
