using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;

public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;

    [SerializeField]
    private float radius = 50;
    [SerializeField]
    private float magnitudeScale = 0.5f;

    [SerializeField]
    private List<Star> allStars;
    public List<Star> AllStars
    {
        get { return allStars; }
        private set { allStars = value; }
    }

    [SerializeField]
    private List<City> allCities;
    public List<City> AllCities
    {
        get { return allCities; }
        private set { allCities = value; }
    }
    public bool showHorizonView = false;

    public GameObject starPrefab;
    public GameObject markerPrefab;
    public GameObject allConstellations;
    public GameObject allMarkers;
    public Material markerMaterial;
    public bool colorByConstellation = true;

    private float simulationTime = 0f;
    private DateTime simulationStartTime = DateTime.Now;
    public float simulationTimeScale = 10f;

    public List<string> cities;

    public string SelectedCity;
    public City currentCity;
    public GameObject cityDropdown;
    public GameObject constellationDropdown;

    private Color colorOrange = new Color(255f/255f, 106f/255f, 0f/255f);
    private Color colorGreen = new Color(76f/255f, 255f/255f, 0f/255f);
    private Color colorBlue = new Color(0f/255f, 148f/255f, 255f/255f);
    private float markerLineWidth = .035f;

    private bool userSpecifiedDateTime = false;
    private int userYear = 2019;
    private int userHour = 0;
    private int userMin = 0;
    private int userDay = 1;
    private DateTime userStartDateTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public TextMeshProUGUI currentDateTimeText;

    // Start is called before the first frame update
    void Start()
    {
        if (allConstellations == null)
        {
            allConstellations = new GameObject();
            allConstellations.name = "Constellations";
        }
        if (starData != null)
        {
            allStars = DataImport.ImportStarData(starData.text);
            Debug.Log(allStars.Count + " stars imported");
        }
        if (cityData != null)
        {
            allCities = DataImport.ImportCityData(cityData.text);
            Debug.Log(allCities.Count + " cities imported");
            cities = allCities.Select(c => c.Name).ToList();
            if (string.IsNullOrEmpty(SelectedCity))
            {
                SelectedCity = "Boston";
            }
            currentCity = allCities.Where(c => c.Name == SelectedCity).FirstOrDefault();
            SelectedCity = currentCity.Name;

            if (cityDropdown)
            {
                cityDropdown.GetComponent<CityDropdown>().InitCityNames(cities, SelectedCity);
            }
        }

        double localSiderialTime = simulationStartTime.Add(TimeSpan.FromHours(currentCity.Lng / 15d)).ToSiderealTime();

        if (starPrefab != null && allStars != null && allStars.Count > 0)
        {
            // get magnitudes and normalize between 1 and 5 to scale stars
            var minMag = allStars.Min(s => s.Mag);
            var maxMag = allStars.Max(s => s.Mag);
            var constellations = new List<string>(allStars.GroupBy(s => s.Constellation).Select(s => s.First().Constellation));  //new Dictionary<string, List<Star>>();
            Debug.Log(minMag + " " + maxMag + " constellations:" + constellations.Count);

            constellations.Sort();
            if (constellationDropdown)
            {
                constellationDropdown.GetComponent<ConstellationDropdown>().InitConstellationNames(constellations, "all");
            }

            foreach (string constellation in constellations)
            {
                List<Star> starsInConstellation = allStars.Where(s => s.Constellation == constellation).ToList();
                GameObject constellationContainer = new GameObject();
                constellationContainer.name = constellation;
                constellationContainer.transform.parent = allConstellations.transform;

                Color constellationColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);

                foreach (Star dataStar in starsInConstellation)
                {
                    GameObject starObject = Instantiate(starPrefab, this.transform.position, Quaternion.identity);
                    StarComponent newStar = starObject.GetComponent<StarComponent>();
                    newStar.starData = dataStar;
                    starObject.name = dataStar.Constellation;
                    if (showHorizonView)
                    {
                        starObject.transform.position = newStar.starData.CalculateHorizonPosition(radius, localSiderialTime, 0);
                    }
                    else
                    {
                        starObject.transform.position = newStar.starData.CalculateEquitorialPosition(radius);
                    }

                    // rescale for magnitude
                    var magScaleValue = ((dataStar.Mag * -1) + maxMag + 1) * magnitudeScale;
                    Vector3 magScale = starObject.transform.localScale * magScaleValue;
                    starObject.transform.localScale = magScale;
                    starObject.transform.LookAt(this.transform);

                    // color by constellation
                    if (colorByConstellation == true)
                    {
                        changeStarColor(starObject, constellationColor);
                        newStar.starColor = constellationColor;
                    }

                    // group by constellation
                    starObject.transform.parent = constellationContainer.transform;

                    // tag it
                    starObject.tag = "Star";
                }
            }
        }

        if (markerPrefab != null)
        {
            AddMarker("NCP", 0f, 90f, localSiderialTime, colorOrange);
            AddMarker("SCP", 0f, -90f, localSiderialTime, colorOrange);
            AddMarker("VE", 0f, 0f, localSiderialTime, colorGreen);
            AddCircumferenceMarker("equator", colorBlue, markerLineWidth);
            AddLineMarker("poleLine", colorOrange, GameObject.Find("NCP"), GameObject.Find("SCP"), markerLineWidth);
        }
    }

    void AddMarker(string markerName, float RA, float dec, double lst, Color color)
    {
        Marker marker = new Marker(markerName, RA, dec);
        GameObject markerObject = Instantiate(markerPrefab, this.transform.position, Quaternion.identity);
        markerObject.transform.parent = allMarkers.transform;
        MarkerComponent newMarker = markerObject.GetComponent<MarkerComponent>();
        newMarker.label.text = markerName;
        newMarker.markerData = marker;
        markerObject.name = markerName;
        changeStarColor(markerObject, color);
        if (showHorizonView)
        {
            markerObject.transform.position = newMarker.markerData.CalculateHorizonPosition(radius, lst, 0);
        }
        else
        {
            markerObject.transform.position = newMarker.markerData.CalculateEquitorialPosition(radius);
        }
    }

    void AddCircumferenceMarker(string markerName, Color color, float lineWidth)
    {
        GameObject circumferenceObject = new GameObject();
        circumferenceObject.name = markerName;
        circumferenceObject.transform.parent = allMarkers.transform;
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
            points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
        }

        lineRendererCircle.SetPositions(points);
    }

    void AddLineMarker(string markerName, Color color, GameObject go1, GameObject go2, float lineWidth)
    {
        GameObject lineObject = new GameObject();
        lineObject.name = markerName;
        lineObject.transform.parent = allMarkers.transform;
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.SetWidth(lineWidth, lineWidth);
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, new Vector3(go1.transform.position.x, go1.transform.position.y, go1.transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(go2.transform.position.x, go2.transform.position.y, go2.transform.position.z));
        lineRenderer.material = markerMaterial;
        lineRenderer.material.color = color;
    }

    void changeStarColor(GameObject starObject, Color nextColor)
    {
        // if using LWRP shader
        starObject.GetComponent<Renderer>().material.SetColor("_BaseColor", nextColor);
        starObject.GetComponent<Renderer>().material.color = nextColor;
    }

    void FixedUpdate()
    {
        if (SelectedCity != currentCity.Name)
        {
            // verify a valid city was entered
            var newCity = allCities.Where(c => c.Name == SelectedCity).First();
            if (newCity != null)
            {
                currentCity = newCity;
            }
            else
            {
                SelectedCity = currentCity.Name;
            }
        }
        if (showHorizonView)
        {
            double lst;
            if (userSpecifiedDateTime)
            {
                lst = userStartDateTime.ToSiderealTime();
                currentDateTimeText.text = userStartDateTime.ToString() + " (UTC)";
            }
            else
            {
                lst = DateTime.Now.ToSiderealTime();
                currentDateTimeText.text = DateTime.UtcNow.ToString() + " (UTC)";
            }
            if (simulationTimeScale > 0)
            {
                simulationTime += simulationTimeScale;
                lst = simulationStartTime.Add(TimeSpan.FromHours(currentCity.Lng / 15d)).AddSeconds(simulationTime).ToSiderealTime();
            }
            foreach (StarComponent starObject in FindObjectsOfType<StarComponent>())
            {
                starObject.gameObject.transform.position = starObject.starData.CalculateHorizonPosition(radius, lst, currentCity.Lat);
                starObject.transform.LookAt(this.transform);
            }
        }
    }

    public DateTime CurrentSimUniversalTime()
    {
        if (userSpecifiedDateTime)
        {
            return userStartDateTime;
        }
        else
        {
            return DateTime.UtcNow;
        }
    }

    public void ChangeCity(string newCity)
    {
        SelectedCity = newCity;
    }

    public void ChangeConstellationHighlight(string highlightConstellation)
    {
        foreach (GameObject starObject in GameObject.FindGameObjectsWithTag("Star"))
        {
            if (starObject.name == highlightConstellation || highlightConstellation == "all")
            {
                Color starColor = starObject.GetComponent<StarComponent>().starColor;
                changeStarColor(starObject, starColor);
            }
            else
            {
                changeStarColor(starObject, Color.white);
            }
        }
    }

    public void ToggleUserTime()
    {
        userSpecifiedDateTime = !userSpecifiedDateTime;
    }

    public void ChangeYear(string newYear)
    {
        userYear = int.Parse(newYear);
        updateUserDateTime();
    }

    public void ChangeDay(float newDay)
    {
        userDay = (int) newDay;
        updateUserDateTime();
    }

    public void ChangeHour(float newHour)
    {
        userHour = (int) Mathf.Floor(newHour);
        userMin = (int) ((newHour % 1) * 60.0f);
        updateUserDateTime();
    }

    private void updateUserDateTime()
    {
        DateTime calculatedStartDateTime = new DateTime(userYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        calculatedStartDateTime = calculatedStartDateTime.AddDays(userDay - 1);
        calculatedStartDateTime = calculatedStartDateTime.AddHours(userHour);
        calculatedStartDateTime = calculatedStartDateTime.AddMinutes(userMin);
        userStartDateTime = calculatedStartDateTime;
    }
}
