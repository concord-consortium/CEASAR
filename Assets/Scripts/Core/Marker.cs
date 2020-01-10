
using UnityEngine;

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
        this.uniqueId = "marker_" + this.Name;
    }
}
