using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkersController : MonoBehaviour
{
    private SimulationManager manager;
    public GameObject markerPrefab;
    public Material markerMaterial;
    private Color colorOrange = new Color(255f / 255f, 106f / 255f, 0f / 255f);
    private Color colorGreen = new Color(76f / 255f, 255f / 255f, 0f / 255f);
    private Color colorBlue = new Color(0f / 255f, 148f / 255f, 255f / 255f);
    public float markerLineWidth = .1f;
    public float markerScale = 1f;
    public bool markersVisible = true;
    public bool poleLineVisible = true;
    public bool equatorLineVisible = true;
    private List<GameObject> markers = new List<GameObject>();

    public void Init()
    {
        manager = SimulationManager.GetInstance();
    }

    public void SetSceneParameters(float markerLineWidth, float markerScale, bool markersVisible, bool poleLineVisible, bool equatorLineVisible)
    {
        this.markerLineWidth = markerLineWidth;
        this.markerScale = markerScale;
        this.markersVisible = markersVisible;
        ShowMarkers(markersVisible, poleLineVisible, equatorLineVisible);
    }

    void CreateMarkers()
    {
        if (markerPrefab != null && markers.Count == 0)
        {
            Vector3 NCP = AddMarker("NCP", 0f, 90f, manager.CurrentSimulationTime.ToSiderealTime(), colorOrange);
            Vector3 SCP = AddMarker("SCP", 0f, -90f, manager.CurrentSimulationTime.ToSiderealTime(), colorOrange);
            AddMarker("VE", 0f, 0f, manager.CurrentSimulationTime.ToSiderealTime(), colorGreen);
            AddCircumferenceMarker("equator", colorBlue, markerLineWidth);
            AddLineMarker("poleLine", colorOrange, NCP, SCP);
        }
    }

    Vector3 AddMarker(string markerName, float RA, float dec, double lst, Color color)
    {
        Marker marker = new Marker(markerName, RA, dec);
        GameObject markerObject = Instantiate(markerPrefab, this.transform);
        markerObject.layer = LayerMask.NameToLayer("Marker");
        markerObject.transform.localScale = markerObject.transform.localScale * markerScale;
        MarkerComponent newMarker = markerObject.GetComponent<MarkerComponent>();
        newMarker.label.text = markerName;
        newMarker.markerData = marker;
        markerObject.name = markerName;
        Utils.SetObjectColor(markerObject, color);

        float radius = manager.InitialRadius + manager.InitialRadius * .1f;
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

        markers.Add(markerObject);
        return markerObject.transform.position;
    }

    void AddCircumferenceMarker(string markerName, Color color, float lineWidth)
    {
        int segments = 360;
        int pointCount = segments + 1;

        GameObject circumferenceObject = new GameObject(markerName);
        circumferenceObject.transform.parent = this.transform;
        circumferenceObject.layer = LayerMask.NameToLayer("Marker");
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
            points[i] = new Vector3(Mathf.Sin(rad) *
                SimulationManager.GetInstance().InitialRadius,
                0,
                Mathf.Cos(rad) * SimulationManager.GetInstance().InitialRadius);
        }

        lineRendererCircle.SetPositions(points);
        markers.Add(circumferenceObject);
    }

    void AddLineMarker(string markerName, Color color, Vector3 p1, Vector3 p2)
    {
        GameObject lineObject = new GameObject(markerName);
        lineObject.transform.parent = this.transform;
        lineObject.layer = LayerMask.NameToLayer("Marker");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        lineRenderer.SetPosition(0, p1);
        lineRenderer.SetPosition(1, p2);
        lineRenderer.material = markerMaterial;
        lineRenderer.material.color = color;
        lineRenderer.startWidth = markerLineWidth;
        lineRenderer.endWidth = markerLineWidth;
        lineRenderer.useWorldSpace = false;
        markers.Add(lineObject);
    }
    // Can be toggled from the UI as well as called on scene change
    public void ShowMarkers(bool showMarkers, bool showPole, bool showEquator)
    {
        markersVisible = showMarkers;
        poleLineVisible = showPole;
        equatorLineVisible = showEquator;
        if (markers.Count == 0)
        {
            CreateMarkers();
        }
        foreach (Transform child in transform)
        {
            LineRenderer lineRenderer = child.GetComponent<LineRenderer>();
            switch (child.name)
            {
                case "NCP":
                    child.gameObject.SetActive(showMarkers);
                    break;
                case "SCP":
                    child.gameObject.SetActive(showMarkers);
                    break;
                case "VE":
                    child.gameObject.SetActive(showMarkers);
                    break;
                case "equator":
                    child.gameObject.SetActive(showEquator);
                    if (lineRenderer != null)
                    {
                        lineRenderer.startWidth = markerLineWidth;
                        lineRenderer.endWidth = markerLineWidth;
                    }
                    break;
                case "poleLine":
                    child.gameObject.SetActive(showPole);
                    if (lineRenderer != null)
                    {
                        lineRenderer.startWidth = markerLineWidth;
                        lineRenderer.endWidth = markerLineWidth;
                    }
                    break;
                default:
                    break;
            }

        }
    }

}
