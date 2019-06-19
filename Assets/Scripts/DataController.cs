using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;
    public TextAsset constellationConnectionData;

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

    public StarComponent starPrefab;

    public GameObject allConstellations;
    private ConstellationsController constellationsController;
    public List<string> constellationFullNames;
    public Constellation constellationPrefab;
    private StarComponent[] allStarComponents;

    private DateTime simulationStartTime = DateTime.Now;
    private double localSiderialStartTime;
    public double LocalSiderialStartTime
    {
        get { return localSiderialStartTime; }
        private set { localSiderialStartTime = value; }
    }

    private DateTime currentSimulationTime = DateTime.Now;
    public DateTime CurrentSimulationTime
    {
        get { return currentSimulationTime; }
    }

    private bool userSpecifiedDateTime = false;
    private DateTime userStartDateTime = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private bool runSimulation = false;

    public List<string> cities;
    public string SelectedCity;
    public City currentCity;

    private Quaternion initialRotation;
    // For storing a copy of the last known time to limit updates
    private double lastTime;

    // Calculated from data
    private float minMag;
    private float maxMag;

    // Scene-specific settings, set via SceneManagerComponent
    private bool colorByConstellation = true;
    private bool showConstellationConnections = true;
    private int maxStars = 10000;
    private float magnitudeScale = 0.5f;
    private float magnitudeThreshold = 4.5f;

    private bool showHorizonView = false;
    private float simulationTimeScale = 10f;
    private float radius = 50;
    public float Radius
    {
        get { return radius; }
        private set { radius = value; }
    }

    private bool isReady = false;


    private struct ConstellationNamePair
    {
        public string shortName;
        public string fullName;
    }

    void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        // Need this so the network UI persists across scenes

        DontDestroyOnLoad(this.gameObject);
    }

    void OnSceneUnloaded(Scene scene)
    {
        this.isReady = false;
    }

    // This is where we set the scene parameters
    public void SetSceneParameters(int maxStars, float magnitudeScale, float magnitudeThreshold,
        bool showHorizonView, float simulationTimeScale, float radius, bool colorByConstellation,
        bool showConstellationConnections)
    {
        this.maxStars = maxStars;
        this.magnitudeScale = magnitudeScale;
        this.magnitudeThreshold = magnitudeThreshold;
        this.showHorizonView = showHorizonView;
        this.simulationTimeScale = simulationTimeScale;
        this.radius = radius;
        this.colorByConstellation = colorByConstellation;
        this.showConstellationConnections = showConstellationConnections;
    }

    public void Init()
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
            allStars = DataImport.ImportStarData(starData.text, maxStars);
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

            var constellationNames = new List<ConstellationNamePair>(allStars.GroupBy(s => s.ConstellationFullName).Select(s => new ConstellationNamePair { shortName = s.First().Constellation, fullName = s.First().ConstellationFullName }));
            Debug.Log(minMag + " " + maxMag + " constellations:" + constellationNames.Count);

            constellationFullNames.Sort();

            foreach (ConstellationNamePair constellationName in constellationNames)
            {

                List<Star> starsInConstellation = allStars.Where(s => s.Constellation == constellationName.shortName).ToList();

                Constellation constellation = Instantiate(constellationPrefab, allConstellations.transform);
                constellation.name = constellationName.shortName.Trim() == "" ? "no-const" : constellationName.shortName;

                // Add constellation name information
                constellation.constellationNameAbbr = constellationName.shortName;
                constellation.constellationNameFull = constellationName.fullName;

                // Add connections
                foreach (ConstellationConnection conn in allConstellationConnections)
                {
                    if (conn.constellationNameAbbr == constellationName.shortName) constellation.AddConstellationConnection(conn);
                }

                Color constellationColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
                if (constellationName.shortName.Trim() == "") constellationColor = Color.white;

                foreach (Star dataStar in starsInConstellation)
                {
                    // Add star object in Equitorial position, child of constellation
                    StarComponent newStar = Instantiate(starPrefab, constellation.transform);
                    newStar.name = constellationName.shortName.Trim() == "" ? "no-const" : dataStar.Constellation;
                    // Add star data, then position, scale and color
                    newStar.Init(constellationsController, dataStar, maxMag, magnitudeScale, radius);

                    // Eventually store constellation color and observed star color separately
                    newStar.SetStarColor(constellationColor, Color.white);
                    // color by constellation
                    if (colorByConstellation == true)
                    {
                        Utils.SetObjectColor(newStar.gameObject, constellationColor);
                        constellation.highlightColor = constellationColor;
                    }

                    allStarComponents[starCount] = newStar;
                    starCount++;

                    // show or hide based on magnitude threshold
                    if (dataStar.Mag > magnitudeThreshold)
                    {
                        newStar.ShowStar(false);
                    }
                    constellation.AddStar(newStar.gameObject);
                }
                constellationsController.AddConstellation(constellation);
            }
        }
    }

    public void UpdateOnSceneLoad()
    {
        // Reset sphere
        isReady = false;
        this.transform.position = new Vector3(0, 0, 0f);
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        this.transform.rotation = Quaternion.identity;
        userSpecifiedDateTime = false;
        runSimulation = false;

        for (int i = 0; i < allStarComponents.Length; i++)
        {
            StarComponent starComponent = allStarComponents[i].gameObject.GetComponent<StarComponent>();
            Utils.SetObjectColor(allStarComponents[i].gameObject, colorByConstellation ? starComponent.constellationColor : Color.white);
            allStarComponents[i].SetStarScale(maxMag, magnitudeScale);
            this.transform.rotation = Quaternion.identity;

            allStarComponents[i].gameObject.transform.position = starComponent.starData.CalculateEquitorialPosition(radius);
            allStarComponents[i].gameObject.transform.LookAt(this.transform);
        }
        if (showHorizonView) positionNCP();
        Debug.Log("updated");
        isReady = true;
    }

    void positionNCP()
    {
        // reset current rotation
        transform.rotation = Quaternion.identity;
        // NCP for selected latitude is due North, elevated at the same angle as latitude
        // this is our axis for siderial daily rotation.
        transform.rotation = Quaternion.Euler(90 - currentCity.Lat, 0, 0);
        initialRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (isReady)
        {
            bool shouldUpdate = false;

            if (SelectedCity != currentCity.Name)
            {
                // verify a valid city was entered
                var newCity = allCities.Where(c => c.Name == SelectedCity).First();
                if (newCity != null)
                {
                    currentCity = newCity;
                    if (showHorizonView) positionNCP();
                    shouldUpdate = true;
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

                // Filter and only update positions if changed time / latitude
                if (lastTime != lst) shouldUpdate = true;

                if (shouldUpdate)
                {
                    float fractionOfDay = ((float)lst / 24) * 360;
                    // TODO: switch from reset & recalculate to just setting the angle around the existing axis
                    transform.rotation = initialRotation;
                    // axis is offset by 90 degrees
                    transform.Rotate(0, fractionOfDay + 90, 0, Space.Self);
                    // Set last timestamp so we only update when changed
                    lastTime = lst;
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
            StarComponent star = allStarComponents[i];
            star.ShowStar(star.starData.Mag < magnitudeThreshold);
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
