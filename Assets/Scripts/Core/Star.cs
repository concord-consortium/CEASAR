using UnityEngine;

public class Star
{
    public string ID; //right now, this is a sequence of characters that combines Bayer and Flamsteed star id
    public string Constellation; //univeral 3-character constellation abbrievation

    public float RA; //right ascension in decimal hours
    public float Dec; //declanation in degrees, positive for northern, negative for southern, 0 to 90
    public float Dist; //star distance in parsecs
    public float Mag; //apparent magnitude (smallest numbers, e.g., negative numbers, are the brightest)
    public float Alt; //Altitude
    public float Azm; //Azimuth

    // other properties for a star
    public float size;
    private float radianRA;
    private float radianDec;
    public bool hidden;

    public string XByerFlamsteed;

    public Star(string Constellation, string XBFlamsteed, float RA, float Dec, float Dist, float Mag)
    {
        this.Constellation = Constellation;
        this.XByerFlamsteed = XBFlamsteed;
        this.RA = RA;
        this.radianRA = RA * 15 * Mathf.Deg2Rad; // RA in hours, so multiply RA by 15 deg / hr
        this.Dec = Dec;
        this.radianDec = this.Dec * Mathf.Deg2Rad;
        this.Dist = Dist;
        this.Mag = Mag;

    }

    // functions to transform equitorial coordinates(RA, Dec) to Cartesian(x, y, z) for the celestial sphere for plotting in the 3D space.
    public Vector3 CalculateEquitorialPosition(float radius)
    {
        var xPos = radius * (Mathf.Cos(radianRA) * Mathf.Cos(radianDec));
        var zPos = radius * (Mathf.Sin(radianRA)) * Mathf.Cos(radianDec); // netlogo vs unity z vs y up
        var yPos = radius * (Mathf.Sin(radianDec));
        return new Vector3(xPos, yPos, zPos);
    }
}

public class Marker
{
    public float RA;
    public float Dec;
}
