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
    public float markerLineWidth = .035f;
    public bool markersVisible = true;
    private List<GameObject> markers = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        dataController = FindObjectOfType<DataController>();
        ShowAllMarkers(markersVisible);
    }

    void CreateMarkers()
    {
        if (markerPrefab != null && markers.Count == 0)
        {
            AddMarker("NCP", 0f, 90f, dataController.LocalSiderialStartTime, colorOrange);
            AddMarker("SCP", 0f, -90f, dataController.LocalSiderialStartTime, colorOrange);
            AddMarker("VE", 0f, 0f, dataController.LocalSiderialStartTime, colorGreen);
            AddCircumferenceMarker("equator", colorBlue, markerLineWidth);
            AddLineMarker("poleLine", colorOrange, GameObject.Find("NCP"), GameObject.Find("SCP"), markerLineWidth);
        }
    }

    void AddMarker(string markerName, float RA, float dec, double lst, Color color)
    {
        Marker marker = new Marker(markerName, RA, dec);
        GameObject markerObject = Instantiate(markerPrefab, this.transform.position, Quaternion.identity);
        markerObject.transform.parent = this.transform;
        MarkerComponent newMarker = markerObject.GetComponent<MarkerComponent>();
        newMarker.label.text = markerName;
        newMarker.markerData = marker;
        markerObject.name = markerName;
        Utils.SetObjectColor(markerObject, color);

        if (dataController.showHorizonView)
        {
            markerObject.transform.position = newMarker.markerData.CalculateHorizonPosition(dataController.Radius, lst, 0);
        }
        else
        {
            markerObject.transform.position = newMarker.markerData.CalculateEquitorialPosition(dataController.Radius);
        }
        markers.Add(markerObject);
    }

    void AddCircumferenceMarker(string markerName, Color color, float lineWidth)
    {
        GameObject circumferenceObject = new GameObject();
        circumferenceObject.name = markerName;
        circumferenceObject.transform.parent = this.transform;
        int segments = 360;
        LineRenderer lineRendererCircle = circumferenceObject.AddComponent<LineRenderer>();
        lineRendererCircle.useWorldSpace = false;
        lineRendererCircle.startWidth = lineWidth;
        lineRendererCircle.endWidth = lineWidth;
        lineRendererCircle.positionCount = segments + 1;
        lineRendererCircle.material = markerMaterial;
        lineRendererCircle.material.color = color;

        int pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
        Vector3[] points = new Vector3[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            float rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * dataController.Radius, 0, Mathf.Cos(rad) * dataController.Radius);
        }

        lineRendererCircle.SetPositions(points);
        markers.Add(circumferenceObject);
    }

    void AddLineMarker(string markerName, Color color, GameObject go1, GameObject go2, float lineWidth)
    {
        GameObject lineObject = new GameObject();
        lineObject.name = markerName;
        lineObject.transform.parent = this.transform;
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, new Vector3(go1.transform.position.x, go1.transform.position.y, go1.transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(go2.transform.position.x, go2.transform.position.y, go2.transform.position.z));
        lineRenderer.material = markerMaterial;
        lineRenderer.material.color = color;
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
