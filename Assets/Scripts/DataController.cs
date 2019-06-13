using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class DataController : MonoBehaviour
{
    private static DataController dataController;
    public static DataController GetInstance()
    {
        return dataController;
    }
    public TextAsset starData;
    public TextAsset cityData;
    public TextAsset constellationConnectionData;

    [SerializeField]
    private float radius = 50;
    public float Radius
    {
        get { return radius; }
        private set { radius = value; }
    }
    [SerializeField]
    private float magnitudeScale = 0.5f;
    [SerializeField]
    private float magnitudeThreshold = 4.5f;
    [SerializeField]
    private float minMag;
    [SerializeField]
    private float maxMag;

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
    private List<ConstellationConnection> allConstellationConnections;
    public List<ConstellationConnection> AllConstellationConnections
    {
        get { return allConstellationConnections; }
        private set { allConstellationConnections = value; }
    }

    public bool showHorizonView = false;

    public GameObject starPrefab;

    public GameObject allConstellations;
    private ConstellationsController constellationsController;
    public List<string> constellationFullNames;
    public GameObject constellationPrefab;
    private StarComponent[] allStarComponents;
    public bool colorByConstellation = true;

    private DateTime simulationStartTime = DateTime.Now;
    private double localSiderialStartTime;
    public double LocalSiderialStartTime
    {
        get { return localSiderialStartTime; }
        private set { localSiderialStartTime = value; }
    }

    private DateTime currentSimulationTime = DateTime.Now;
    public float simulationTimeScale = 10f;
    private bool userSpecifiedDateTime = false;
    private DateTime userStartDateTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private bool runSimulation = false;

    public List<string> cities;
    public string SelectedCity;
    public City currentCity;

    private class ConstellationNamePair
    {
        public string shortName;
        public string fullName;
    }

    void Awake()
    {
         if (dataController == null)
         {
             DontDestroyOnLoad (this.gameObject);
             dataController = this;
             Init();
         }
         else if (dataController != this)
         {
             Destroy (gameObject);
         }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void UpdateOnSceneLoad(bool showHorizon, float ts, float rad, float mag, bool showObjs, bool consColor)
    {
        showHorizonView = showHorizon;
        simulationTimeScale = ts;
        radius = rad;
        magnitudeScale = mag;
        // use an array for speed of access
        for (int i = 0; i < allStarComponents.Length; i++)
        {
            StarComponent starComponent = allStarComponents[i].gameObject.GetComponent<StarComponent>();
            Utils.SetObjectColor(allStarComponents[i].gameObject, consColor ? starComponent.constellationColor : Color.white);
            if (showHorizon)
            {
                allStarComponents[i].gameObject.transform.position = starComponent.starData.CalculateHorizonPosition(radius, localSiderialStartTime, 0);
            }
            else
            {
                allStarComponents[i].gameObject.transform.position = starComponent.starData.CalculateEquitorialPosition(radius);
            }
            var magScaleValue = ((starComponent.starData.Mag * -1) + maxMag + 1) * magnitudeScale;
            Vector3 magScale = new Vector3(1f, 1f, 1f) * magScaleValue;
            allStarComponents[i].gameObject.transform.localScale = magScale;
            allStarComponents[i].gameObject.transform.LookAt(this.transform);
        }
        allConstellations.GetComponent<ConstellationsController>().ShowAllConstellations(showObjs);
        MarkersController markersController = FindObjectOfType<MarkersController>();
        markersController.ShowAllMarkers(showObjs);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // restore sphere transform
        this.transform.position = new Vector3(0, 0, 0f);
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        this.transform.rotation = Quaternion.identity;
        // turn off simulation
        userSpecifiedDateTime = false;
        runSimulation = false;
        if (scene.name == "Horizon")
        {
            UpdateOnSceneLoad(true, 1, 100, .5f, false, true);
        }
        else if (scene.name == "Planets")
        {
            UpdateOnSceneLoad(false, 10, 500f, 1f, false, false);
        }
        else if (scene.name == "Stars")
        {
            UpdateOnSceneLoad(false, 10, 75f, .3f, true, true);
        }
        else
        {
            UpdateOnSceneLoad(false, 10, 100f, .2f, false, true);
        }
    }

    private void Init()
    {
        if (allConstellations == null)
        {
            allConstellations = new GameObject();
            allConstellations.name = "Constellations";
            allConstellations.AddComponent<ConstellationsController>();
        }
        constellationsController = allConstellations.GetComponent<ConstellationsController>();
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
        }
        if (constellationConnectionData != null)
        {
            allConstellationConnections = DataImport.ImportConstellationConnectionData(constellationConnectionData.text);
            Debug.Log(allConstellationConnections.Count + " connections imported");
        }

        localSiderialStartTime = simulationStartTime.Add(TimeSpan.FromHours(currentCity.Lng / 15d)).ToSiderealTime();
        int starCount = 0;
        if (starPrefab != null && allStars != null && allStars.Count > 0)
        {
            // get magnitudes and normalize between 1 and 5 to scale stars
            minMag = allStars.Min(s => s.Mag);
            maxMag = allStars.Max(s => s.Mag);
            constellationFullNames = new List<string>(allStars.GroupBy(s => s.ConstellationFullName).Select(s => s.First().ConstellationFullName));
            var constellationNames = new List<ConstellationNamePair>(allStars.GroupBy(s => s.ConstellationFullName).Select(s => new ConstellationNamePair{shortName = s.First().Constellation, fullName = s.First().ConstellationFullName}));
            Debug.Log(minMag + " " + maxMag + " constellations:" + constellationNames.Count);

            constellationFullNames.Sort();

            foreach (ConstellationNamePair constellationName in constellationNames)
            {
                List<Star> starsInConstellation = allStars.Where(s => s.Constellation == constellationName.shortName).ToList();

                GameObject constellationContainer = Instantiate(constellationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                constellationContainer.name = constellationName.shortName.Trim() == "" ? "no-const" : constellationName.shortName;
                constellationContainer.transform.parent = allConstellations.transform;
                Constellation constellation = constellationContainer.GetComponent<Constellation>();
                constellation.constellationNameAbbr = constellationName.shortName;
                constellation.constellationNameFull = constellationName.fullName;
                foreach (ConstellationConnection conn in allConstellationConnections)
                {
                    if (conn.constellationNameAbbr == constellationName.shortName) constellation.AddConstellationConnection(conn);
                }

                Color constellationColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
                if (constellationName.shortName.Trim() == "") constellationColor = Color.white;

                foreach (Star dataStar in starsInConstellation)
                {
                    GameObject starObject = Instantiate(starPrefab, this.transform.position, Quaternion.identity);
                    StarComponent newStar = starObject.GetComponent<StarComponent>();
                    newStar.Init(constellationsController);
                    newStar.starData = dataStar;
                    starObject.name = constellationName.shortName.Trim() == "" ? "no-const" : dataStar.Constellation;
                    if (showHorizonView)
                    {
                        starObject.transform.position = newStar.starData.CalculateHorizonPosition(radius, localSiderialStartTime, 0);
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
                        constellation.highlightColor = constellationColor;
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
                    constellation.AddStar(starObject);
                }
                constellationsController.AddConstellation(constellation);
            }
            if (!showHorizonView && colorByConstellation) constellationsController.HighlightAllConstellations(true);
        }
    }

    void showStar(GameObject starObject, bool show)
    {
        Renderer rend = starObject.GetComponent<Renderer>();
        Collider coll = starObject.GetComponent<Collider>();
        rend.enabled = show;
        coll.enabled = show;
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
