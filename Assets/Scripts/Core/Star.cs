using UnityEngine;
using System;

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
        this.uniqueId = "star_" + RA.ToString() + "_" + Dec.ToString();
    }
}
