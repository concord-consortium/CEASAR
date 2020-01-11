using System;
using System.Collections.Generic;
using SunCalcNet;
using UnityEngine;
using SunCalcNet.Model;

public class SunPositionController : MonoBehaviour
{
    public GameObject sun;
    private DataController dataController;
    private City currentCity;

    public Material lineMaterial;
    private LineRenderer sunArcLine;
    int secondsInADay = 24 * 60 * 60;
    int desiredLineNodeCount = 60;
    float xScale = 0.005f;
    float radius = 100;
    // Start is called before the first frame update
    void Start()
    {
        dataController = SimulationManager.GetInstance().DataControllerComponent;
        currentCity = dataController.currentCity;
        //var solarPosition = CalculateSunPosition(DateTime.Now, dataController.currentCity.Lat, dataController.currentCity.Lng);

        // move sun game object
        sun.transform.position =
            getSolarPosition(DateTime.Now, dataController.currentCity.Lat, dataController.currentCity.Lng);
            // Utils.CalculatePositionByAzAlt(solarPosition.Azimuth, solarPosition.Altitude, dataController.Radius);
            // // new Vector3((12 - DateTime.Now.Hour) * 10, (float)solarPosition.Altitude, transform.position.z + 20);

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
        List<Vector3> points = new List<Vector3>();
        DateTime midnight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        for (int i = 0; i < secondsInADay; i += (secondsInADay / desiredLineNodeCount))
        {
            DateTime t = midnight.AddSeconds(i);
            points.Add(getSolarPosition(t, dataController.currentCity.Lat, dataController.currentCity.Lng));
        }
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
            List<Vector3> points = new List<Vector3>();
            DateTime d = dataController.CurrentSimUniversalTime();
            DateTime midnight = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            for (int i = 0; i < secondsInADay; i += (secondsInADay / desiredLineNodeCount))
            {
                DateTime t = midnight.AddSeconds(i);
                points.Add(getSolarPosition(t, dataController.currentCity.Lat, dataController.currentCity.Lng));

            }
            sunArcLine.SetPositions(points.ToArray());
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (currentCity != dataController.currentCity)
        {
            currentCity = dataController.currentCity;
            
        }
        renderSunArc();

        if (sun != null) sun.transform.position = getSolarPosition(dataController.CurrentSimUniversalTime(), dataController.currentCity.Lat, dataController.currentCity.Lng);
    }

    Vector3 getSolarPosition(DateTime t, double lat, double lng)
    {
        var solarPosition = SunCalc.GetSunPosition(t, lat, lng);

        return Utils.CalculatePositionByAzAlt(solarPosition.Azimuth, solarPosition.Altitude, dataController.Radius);

    }
}