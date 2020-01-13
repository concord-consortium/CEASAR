using System;
using System.Collections.Generic;
using SunCalcNet;
using UnityEngine;

public class SunPositionController : MonoBehaviour
{
    public GameObject sun;
    private DataController dataController;

    public Material lineMaterial;
    private LineRenderer sunArcLine;
    int secondsInADay = 24 * 60 * 60;
    int desiredLineNodeCount = 60;
    
    // Start is called before the first frame update
    void Start()
    {
        dataController = SimulationManager.GetInstance().DataControllerComponent;

        // move sun game object
        sun.transform.position =
            getSolarPosition(DateTime.Now, dataController.currentCity.Lat, dataController.currentCity.Lng);

        renderSunArc();
    }

    void createSunArc()
    {
        if (sunArcLine == null)
        {
            sunArcLine = gameObject.AddComponent<LineRenderer>();
        }
        else
        {
            sunArcLine.positionCount = 0;
        }

        List<Vector3> points = getArcPoints();
        sunArcLine.positionCount = points.Count;
        sunArcLine.SetPositions(points.ToArray());
        sunArcLine.material = lineMaterial;
        sunArcLine.startWidth = 0.2f;
        sunArcLine.endWidth = 0.2f;
    }
    void renderSunArc()
    {
        if (sunArcLine == null)
        {
            createSunArc();
        }
        else
        {
            sunArcLine.SetPositions(getArcPoints().ToArray());
        }
    }

    List<Vector3> getArcPoints()
    {
        List<Vector3> points = new List<Vector3>();
        DateTime d = dataController.CurrentSimUniversalTime;
        DateTime midnight = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
        for (int i = 0; i < secondsInADay; i += (secondsInADay / desiredLineNodeCount))
        {
            DateTime t = midnight.AddSeconds(i);
            points.Add(getSolarPosition(t, dataController.currentCity.Lat, dataController.currentCity.Lng));
        }
        return points;
    }
    // Update is called once per frame
    void Update()
    {
        renderSunArc();

        if (sun != null)
        {
            sun.transform.position = getSolarPosition(dataController.CurrentSimUniversalTime, dataController.currentCity.Lat, dataController.currentCity.Lng);
            sun.transform.LookAt(new Vector3(0,0,0));
        }
        
    }

    Vector3 getSolarPosition(DateTime t, double lat, double lng)
    {
        var solarPosition = SunCalc.GetSunPosition(t, lat, lng);

        return Utils.CalculatePositionByAzAlt(solarPosition.Azimuth, solarPosition.Altitude, dataController.Radius);
    }
}