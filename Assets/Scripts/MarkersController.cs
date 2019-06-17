using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersController : MonoBehaviour
{
    private DataController dataController;
    public GameObject markerPrefab;
    public Material markerMaterial;
    private Color colorOrange = new Color(255f / 255f, 106f / 255f, 0f / 255f);
    private Color colorGreen = new Color(76f / 255f, 255f / 255f, 0f / 255f);
    private Color colorBlue = new Color(0f / 255f, 148f / 255f, 255f / 255f);
    public float markerLineWidth = .1f;
    public bool markersVisible = true;
    public bool poleLineVisible = true;
    private List<GameObject> markers = new List<GameObject>();

    public void Init()
    {
        dataController = DataController.GetInstance();
        ShowAllMarkers(markersVisible);
    }

    void CreateMarkers()
    {
        if (markerPrefab != null && markers.Count == 0)
        {
            Vector3 NCP = AddMarker("NCP", 0f, 90f, dataController.LocalSiderialStartTime, colorOrange);
            Vector3 SCP = AddMarker("SCP", 0f, -90f, dataController.LocalSiderialStartTime, colorOrange);
            AddMarker("VE", 0f, 0f, dataController.LocalSiderialStartTime, colorGreen);
            AddCircumferenceMarker("equator", colorBlue, markerLineWidth);
            if (poleLineVisible) AddLineMarker("poleLine", colorOrange, NCP, SCP);
        }
    }

    Vector3 AddMarker(string markerName, float RA, float dec, double lst, Color color)
    {
        Marker marker = new Marker(markerName, RA, dec);
        GameObject markerObject = Instantiate(markerPrefab, this.transform);
        MarkerComponent newMarker = markerObject.GetComponent<MarkerComponent>();
        newMarker.label.text = markerName;
        newMarker.markerData = marker;
        markerObject.name = markerName;
        Utils.SetObjectColor(markerObject, color);

        if (dataController.UseNCPRotation)
        {
            float radius = dataController.Radius + 10;
            // Set marker positions in Equitorial position and move with celestial sphere
            switch (markerName)
            {
                case "NCP":
                    markerObject.transform.position = radius * new Vector3(0, 1, 0);
                    break;
                case "SCP":
                    markerObject.transform.position = radius * new Vector3(0, -1, 0);
                    break;
                case "VE":
                    markerObject.transform.position = radius * new Vector3(1, 0, 0);
                    break;
            }
        }
        else
        {
            if (dataController.showHorizonView)
            {
                markerObject.transform.position = newMarker.markerData.CalculateHorizonPosition(dataController.Radius, lst, 0);
            }
            else
            {
                markerObject.transform.position = newMarker.markerData.CalculateEquitorialPosition(dataController.Radius);
            }
        }
        markers.Add(markerObject);
        return markerObject.transform.position;
    }

    void AddCircumferenceMarker(string markerName, Color color, float lineWidth)
    {
        int segments = 360;
        int pointCount = segments + 1;

        GameObject circumferenceObject = new GameObject(markerName);
        circumferenceObject.transform.parent = this.transform;

        LineRenderer lineRendererCircle = circumferenceObject.AddComponent<LineRenderer>();
        lineRendererCircle.useWorldSpace = false;
        lineRendererCircle.startWidth = lineWidth;
        lineRendererCircle.endWidth = lineWidth;
        lineRendererCircle.material = markerMaterial;

        lineRendererCircle.positionCount = pointCount;
        lineRendererCircle.material.color = color;
        // add extra point to make startpoint and endpoint the same to close the circle
        Vector3[] points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * dataController.Radius, 0, Mathf.Cos(rad) * dataController.Radius);
        }

        lineRendererCircle.SetPositions(points);
        markers.Add(circumferenceObject);
    }

    void AddLineMarker(string markerName, Color color, Vector3 p1, Vector3 p2)
    {
        GameObject lineObject = new GameObject(markerName);
        lineObject.transform.parent = this.transform;
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.SetPosition(0, p1);
        lineRenderer.SetPosition(1, p2);
        lineRenderer.material = markerMaterial;
        lineRenderer.material.color = color;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = markerLineWidth;
        lineRenderer.useWorldSpace = false;
        markers.Add(lineObject);
    }

    public void ShowAllMarkers(bool show)
    {
        markersVisible = show;
        if (markersVisible) CreateMarkers();
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(markersVisible);
        }
    }

}
