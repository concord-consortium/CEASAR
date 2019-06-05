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
}
