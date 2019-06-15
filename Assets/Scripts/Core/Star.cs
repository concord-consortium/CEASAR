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

    // TODO: remove these completely eventually
    // functions to transform equitorial coordinates(RA, Dec) to Cartesian(x, y, z) for the celestial sphere for plotting in the 3D space.
    public Vector3 CalculateEquitorialPosition(float radius)
    {
        return Utils.CalculateEquitorialPosition(radianRA, radianDec, radius);
    }

    // calculate the position of stars at a point on Earth
    public Vector3 CalculateHorizonPosition(float radius, double currentSiderialTime, float observerLatitude)
    {
        return Utils.CalculateHorizonPosition(RA, radianDec, radius, currentSiderialTime, observerLatitude);
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
    public string ConstellationFullName; //full human readable constellation name
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

    public Star(int Hip, string ConstellationAbbr, string ConstellationFull, string ProperName, string XBFlamsteed, string FlamsteedDes, string BayerDes, float RA, float RadianRA, float Dec, float RadianDec, float Dist, float Mag, float AbsMag, string Spectrum, float ColorIndex)
    {
        this.Hipparcos = Hip;
        this.Constellation = ConstellationAbbr;
        this.ConstellationFullName = ConstellationFull;
        this.ProperName = ProperName;
        this.XBayerFlamsteed = XBFlamsteed;
        this.FlamsteedDesignation = FlamsteedDes;
        this.BayerDesignation = BayerDes;
        this.RA = RA;
        this.radianRA = RadianRA; //(RA * 15 * Mathf.Deg2Rad) RA in hours, so multiply RA by 15 deg / hr
        this.Dec = Dec;
        this.radianDec = RadianDec; //(Dec * Mathf.Deg2Rad)
        this.Dist = Dist;
        this.Mag = Mag;
        this.AbsMag = AbsMag;
        this.Spectrum = Spectrum;
        this.ColorIndex = ColorIndex;
    }
}
