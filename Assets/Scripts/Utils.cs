using System;
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
                   (oldT.rotation.z != newT.rotation.z) ||
                    (oldT.rotation.w != newT.rotation.w)) return true;
        return false;
    }
    public static Vector3 NetworkPosToPosition(NetworkPosition pos)
    {
        return new Vector3(pos.x, pos.y, pos.x);
    }
    public static Quaternion NetworkRotToRotation(NetworkRotation rot)
    {
        return new Quaternion(rot.x, rot.y, rot.x, rot.w);
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