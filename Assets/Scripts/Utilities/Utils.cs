﻿using System;
using UnityEngine;
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
    public static Vector3 NetworkPosToPosition(NetworkVector3 pos)
    {
        return new Vector3(pos.x, pos.y, pos.z);
    }
    public static Quaternion NetworkRotToRotation(NetworkVector3 rot)
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

    public static Vector3 CalculateHorizonPosition(float RA, float radianDec, float radius, double currentSiderialTime, float observerLatitude)
    {
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