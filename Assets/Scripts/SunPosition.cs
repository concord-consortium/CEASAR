using System;
using System.Collections.Generic;
using UnityEngine;

public class SunPosition : MonoBehaviour
{
    public GameObject sun;
    public DataController dataController;
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
        if (dataController == null) dataController = FindObjectOfType<DataController>();
        currentCity = dataController.currentCity;
        var solarPosition = CalculateSunPosition(DateTime.Now, dataController.currentCity.Lat, dataController.currentCity.Lng);

        // move sun game object
        sun.transform.position = new Vector3((12 - DateTime.Now.Hour) * 10, (float)solarPosition.Altitude, transform.position.z + 20);

        renderSunArc();
    }
    void renderSunArc()
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
            var solarPosition = CalculateSunPosition(t, dataController.currentCity.Lat, dataController.currentCity.Lng);
            points.Add(solarPosToWorld(solarPosition));

            // Debug.LogFormat("Result ==> Time: {0}, Altitude: {1}, Azimuth :{2}", t.ToShortTimeString(), solarPosition.Altitude, solarPosition.Azimuth);
        }
        sunArcLine.positionCount = points.Count;
        sunArcLine.SetPositions(points.ToArray());
        sunArcLine.material = lineMaterial;
        sunArcLine.startWidth = 0.2f;
        sunArcLine.endWidth = 0.2f;
    }
    // Update is called once per frame
    void Update()
    {
        if (currentCity != dataController.currentCity)
        {
            currentCity = dataController.currentCity;
            renderSunArc();
        }
        var solarPosition = CalculateSunPosition(dataController.CurrentSimUniversalTime(), dataController.currentCity.Lat, dataController.currentCity.Lng);

        if (sun != null) sun.transform.position = solarPosToWorld(solarPosition);
    }
    Vector3 solarPosToWorld(SolarPosition pos)
    {
        Vector3 p = transform.position - (Vector3.forward * radius * 0.5f);
        p.x = calculateX((float)pos.Azimuth); //((secondsInADay / 2) - i) * xScale;
        p.y = calculateY((float)pos.Altitude);//(float)solarPosition.Altitude;
        return p;
    }
    float calculateX(float azimuth)
    {
        return radius * Mathf.Sin(azimuth);
    }
    float calculateY(float altitude)
    {
        return radius * Mathf.Sin(altitude);
    }
    /// <summary>
    /// Calculates the sun position. calculates the suns "position" based on a
    /// given date and time in local time, latitude and longitude
    /// expressed in decimal degrees.It is based on the method
    /// found here:
    /// http://www.astro.uio.no/~bgranslo/aares/calculate.html
    /// The calculation is only satisfiably correct for dates in
    /// the range March 1 1900 to February 28 2100.
    /// </summary>
    /// <returns>The sun position.</returns>
    /// <param name="dateTime">Time and date in local time</param>
    /// <param name="latitude">Latitude expressed in decimal degrees</param>
    /// <param name="longitude">Longitude expressed in decimal degrees</param>
    SolarPosition CalculateSunPosition(
       DateTime dateTime, float latitude, float longitude)
    {
        // Convert to UTC
        dateTime = dateTime.ToUniversalTime();

        // Number of days from J2000.0.
        double julianDate = 366 * dateTime.Year -
            (int)((7.0 / 4.0) * (dateTime.Year +
            (int)((dateTime.Month + 9.0) / 12.0))) +
            (int)((275.0 * dateTime.Month) / 9.0) +
            dateTime.Day - 730530.5;

        double julianCenturies = julianDate / 36525.0;

        // Sidereal Time
        double siderealTimeHours = 6.6974 + 2400.0013 * julianCenturies;

        double siderealTimeUT = siderealTimeHours +
            (366.2422 / 365.2422) * dateTime.TimeOfDay.TotalHours;

        double siderealTime = siderealTimeUT * 15 + longitude;

        // Refine to number of days (fractional) to specific time.
        julianDate += dateTime.TimeOfDay.TotalHours / 24.0;
        julianCenturies = julianDate / 36525.0;

        // Solar Coordinates
        double meanLongitude = CorrectAngle(Mathf.Deg2Rad *
            (280.466 + 36000.77 * julianCenturies));

        double meanAnomaly = CorrectAngle(Mathf.Deg2Rad *
            (357.529 + 35999.05 * julianCenturies));

        double equationOfCenter = Mathf.Deg2Rad * ((1.915 - 0.005 * julianCenturies) *
            Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));

        double elipticalLongitude =
            CorrectAngle(meanLongitude + equationOfCenter);

        double obliquity = (23.439 - 0.013 * julianCenturies) * Mathf.Deg2Rad;

        // Right Ascension
        double rightAscension = Math.Atan2(
            Math.Cos(obliquity) * Math.Sin(elipticalLongitude),
            Math.Cos(elipticalLongitude));

        double declination = Math.Asin(
            Math.Sin(rightAscension) * Math.Sin(obliquity));

        // Horizontal Coordinates
        double hourAngle = CorrectAngle(siderealTime * Mathf.Deg2Rad) - rightAscension;

        if (hourAngle > Math.PI)
        {
            hourAngle -= 2 * Math.PI;
        }

        double altitude = Math.Asin(Math.Sin(latitude * Mathf.Deg2Rad) *
            Math.Sin(declination) + Math.Cos(latitude * Mathf.Deg2Rad) *
            Math.Cos(declination) * Math.Cos(hourAngle));

        // Nominator and denominator for calculating Azimuth
        // angle. Needed to test which quadrant the angle is in.
        double aziNom = -Math.Sin(hourAngle);
        double aziDenom =
            Math.Tan(declination) * Math.Cos(latitude * Mathf.Deg2Rad) -
            Math.Sin(latitude * Mathf.Deg2Rad) * Math.Cos(hourAngle);

        double azimuth = Math.Atan(aziNom / aziDenom);

        if (aziDenom < 0) // In 2nd or 3rd quadrant
        {
            azimuth += Math.PI;
        }
        else if (aziNom < 0) // In 4th quadrant
        {
            azimuth += 2 * Math.PI;
        }

        // Altitude
        // Console.WriteLine("Altitude: " + altitude * Rad2Deg);

        // Azimut
        //Console.WriteLine("Azimuth: " + azimuth * Rad2Deg);

        return new SolarPosition { Altitude = altitude, Azimuth = azimuth };
    }
    double CorrectAngle(double angleInRadians)
    {
        if (angleInRadians < 0)
        {
            return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
        }
        else if (angleInRadians > 2 * Math.PI)
        {
            return angleInRadians % (2 * Math.PI);
        }
        else
        {
            return angleInRadians;
        }
    }
}
public struct SolarPosition
{
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
}