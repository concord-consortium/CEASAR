using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;

public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;
    public TextAsset constellationAbbrData;

    [SerializeField]
    private float radius = 50;
    [SerializeField]
    private float magnitudeScale = 0.5f;
    [SerializeField]
    private float magnitudeThreshold = 4.5f;

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

    [SerializeField]
    private List<ConstellationAbbr> allConstellationAbbrs;
    public List<ConstellationAbbr> AllConstellationAbbrs
    {
        get { return allConstellationAbbrs; }
        private set { allConstellationAbbrs = value; }
    }


    public bool showHorizonView = false;

    public GameObject starPrefab;
    public GameObject markerPrefab;
    public GameObject allConstellations;
    private StarComponent[] allStarComponents;
    public GameObject allMarkers;
    public Material markerMaterial;
    public bool colorByConstellation = true;

    private DateTime simulationStartTime = DateTime.Now;
    private DateTime currentSimulationTime = DateTime.Now;
    public float simulationTimeScale = 10f;
    private bool userSpecifiedDateTime = false;
    private DateTime userStartDateTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private bool runSimulation = false;

    public List<string> cities;

    public string SelectedCity;
    public City currentCity;
    public GameObject cityDropdown;
    public GameObject constellationDropdown;

    private Color unnamedColor = new Color(128f / 255f, 128f / 255f, 128f / 255f);
    private Color colorOrange = new Color(255f / 255f, 106f / 255f, 0f / 255f);
    private Color colorGreen = new Color(76f / 255f, 255f / 255f, 0f / 255f);
    private Color colorBlue = new Color(0f / 255f, 148f / 255f, 255f / 255f);
    private float markerLineWidth = .035f;

    public GameObject starInfoPanel;

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
            allStarComponents = new StarComponent[allStars.Count];
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
        if (constellationAbbrData != null)
        {
            allConstellationAbbrs = DataImport.ImportConstellationAbbreviationData(constellationAbbrData.text);
        }


        double localSiderialTime = simulationStartTime.Add(TimeSpan.FromHours(currentCity.Lng / 15d)).ToSiderealTime();
        int starCount = 0;
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
                constellationContainer.name = constellation.Trim() == "" ? "no-const" : constellation;
                constellationContainer.transform.parent = allConstellations.transform;

                Color constellationColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
                if (constellation.Trim() == "") constellationColor = unnamedColor;

                foreach (Star dataStar in starsInConstellation)
                {
                    GameObject starObject = Instantiate(starPrefab, this.transform.position, Quaternion.identity);
                    StarComponent newStar = starObject.GetComponent<StarComponent>();
                    newStar.starData = dataStar;
                    starObject.name = constellation.Trim() == "" ? "no-const" : dataStar.Constellation;
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
                    // Eventually store constellation color and observed star color separately
                    newStar.SetStarColor(constellationColor, Color.white);
                    // color by constellation
                    if (colorByConstellation == true)
                    {
                        Utils.SetObjectColor(starObject, constellationColor);
                    }

                    allStarComponents[starCount] = newStar;
                    starCount++;

                    // group by constellation
                    starObject.transform.parent = constellationContainer.transform;

                    // show or hide based on magnitude threshold
                    if (dataStar.Mag > magnitudeThreshold)
                    {
                        showStar(starObject, false);
                    }
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

    void showStar(GameObject starObject, bool show)
    {
        Renderer rend = starObject.GetComponent<Renderer>();
        Collider coll = starObject.GetComponent<Collider>();
        rend.enabled = show;
        coll.enabled = show;
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
        Utils.SetObjectColor(markerObject, color);

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
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPosition(0, new Vector3(go1.transform.position.x, go1.transform.position.y, go1.transform.position.z));
        lineRenderer.SetPosition(1, new Vector3(go2.transform.position.x, go2.transform.position.y, go2.transform.position.z));
        lineRenderer.material = markerMaterial;
        lineRenderer.material.color = color;
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
                if (simulationTimeScale > 0 && runSimulation)
                {
                    currentSimulationTime = currentSimulationTime.AddSeconds(Time.deltaTime * simulationTimeScale);
                }
                else
                {
                    currentSimulationTime = userStartDateTime;
                }
                lst = currentSimulationTime.ToSiderealTime();
            }
            else
            {
                lst = DateTime.Now.ToSiderealTime();
            }

            // use an array for speed of access, only update if visible
            for (int i = 0; i < allStarComponents.Length; i++)
            {
                if (allStarComponents[i].gameObject.GetComponent<Renderer>().enabled)
                {
                    allStarComponents[i].gameObject.transform.position = allStarComponents[i].starData.CalculateHorizonPosition(radius, lst, currentCity.Lat);
                    allStarComponents[i].transform.LookAt(this.transform);
                }
            }

        }
    }

    public DateTime CurrentSimUniversalTime()
    {
        if (userSpecifiedDateTime)
        {
            return currentSimulationTime;
        }
        else
        {
            return DateTime.UtcNow;
        }
    }

    public void SetSelectedCity(string newCity)
    {
        SelectedCity = newCity;
    }

    public void ChangeConstellationHighlight(string highlightConstellation)
    {
        foreach (GameObject starObject in GameObject.FindGameObjectsWithTag("Star"))
        {
            if (starObject.name == highlightConstellation || highlightConstellation == "all")
            {
                Color constellationColor = starObject.GetComponent<StarComponent>().constellationColor;

                Utils.SetObjectColor(starObject, constellationColor);
            }
            else
            {
                Color starColor = starObject.GetComponent<StarComponent>().starColor;
                Utils.SetObjectColor(starObject, starColor);
            }
        }
        constellationDropdown.GetComponent<ConstellationDropdown>().UpdateConstellationSelection(highlightConstellation);
    }

    public void ChangeStarSelection(GameObject selectedStar)
    {
        if (starInfoPanel)
        {
            StarComponent starComponent = selectedStar.GetComponent<StarComponent>();
            starInfoPanel.GetComponent<StarInfoPanel>().UpdateStarInfoPanel(starComponent.starData.XBayerFlamsteed,
                                                                            starComponent.starData.Mag.ToString(),
                                                                            starComponent.starData.Constellation);
            starInfoPanel.GetComponent<WorldToScreenPos>().UpdatePosition(selectedStar);
        }
    }

    public void SetMagnitudeThreshold(float newVal)
    {
        magnitudeThreshold = newVal;
        for (int i = 0; i < allStarComponents.Length; i++)
        {
            showStar(allStarComponents[i].gameObject, allStarComponents[i].starData.Mag < magnitudeThreshold);
        }
    }

    public void ToggleUserTime()
    {
        userSpecifiedDateTime = !userSpecifiedDateTime;
    }
    public void ToggleRunSimulation()
    {
        runSimulation = !runSimulation;
        if (runSimulation)
        {
            currentSimulationTime = userStartDateTime;
        }
    }

    public void SetUserStartDateTime(DateTime newStartDateTime)
    {
        userStartDateTime = newStartDateTime;
    }


    public void SetSimulationTimeScale(float newVal)
    {
        simulationTimeScale = newVal;
    }
}
