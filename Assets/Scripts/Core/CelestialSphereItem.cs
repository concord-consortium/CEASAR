
using UnityEngine;

public abstract class CelestialSphereItem
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