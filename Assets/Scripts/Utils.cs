using System;
using UnityEngine;
public class Utils
{
    protected Utils()
    {
    }
    private static Utils instance;
    public static Utils GetInstance()
    {
        return instance ?? (instance = new Utils());
    }

    public void SetObjectColor(GameObject go, Color newColor)
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
