using System;
using System.Collections.Generic;
using SunCalcNet;
using UnityEngine;

public class MoonPositionController : MonoBehaviour
{
    public GameObject moon;
    private SimulationManager manager;

    public bool showMoonArc = false;
    public Material lineMaterial;
    private LineRenderer moonArcLine;
    int secondsInADay = 24 * 60 * 60;
    int desiredLineNodeCount = 60;
    
    // Start is called before the first frame update
    void Start()
    {
        manager = SimulationManager.Instance;
        // move sun game object
        moon.transform.position =
            getLunarPosition(manager.CurrentSimulationTime,manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);

        renderMoonArc();
    }

    void createMoonArc()
    {
        if (moonArcLine == null)
        {
            moonArcLine = gameObject.AddComponent<LineRenderer>();
        }
        else
        {
            moonArcLine.positionCount = 0;
        }

        List<Vector3> points = getArcPoints();
        moonArcLine.positionCount = points.Count;
        moonArcLine.SetPositions(points.ToArray());
        moonArcLine.material = lineMaterial;
        moonArcLine.startWidth = 0.2f;
        moonArcLine.endWidth = 0.2f;
    }
    void renderMoonArc()
    {
        if (!showMoonArc) return;
        if (moonArcLine == null)
        {
            createMoonArc();
        }
        else
        {
            moonArcLine.SetPositions(getArcPoints().ToArray());
        }
    }

    List<Vector3> getArcPoints()
    {
        List<Vector3> points = new List<Vector3>();
        DateTime d = SimulationManager.Instance.CurrentSimulationTime;
        DateTime midnight = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
        for (int i = 0; i < secondsInADay; i += (secondsInADay / desiredLineNodeCount))
        {
            DateTime t = midnight.AddSeconds(i);
            points.Add(getLunarPosition(t, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude));
        }
        return points;
    }
    // Update is called once per frame
    void Update()
    {
        renderMoonArc();

        if (moon != null) moon.transform.position = getLunarPosition(manager.CurrentSimulationTime, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);
    }

    Vector3 getLunarPosition(DateTime t, double lat, double lng)
    {
        var lunarPosition = MoonCalc.GetMoonPosition(t, lat, lng);

        return Utils.CalculatePositionByAzAlt(lunarPosition.Azimuth, lunarPosition.Altitude, manager.SceneRadius);
    }
}
