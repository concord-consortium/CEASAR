using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;


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
    public Material starMaterial;
    
    public GameObject allConstellations;
    private ConstellationsController constellationsController;
    public List<string> constellationFullNames;
    public Constellation constellationPrefab;
    private Dictionary<string, StarComponent> allStarComponents;

    public StarComponent GetStarById(string uniqueId)
    {
        return allStarComponents[uniqueId];
    }
    private bool runSimulation = false;

    public List<string> cities;
    private string startCity = SimulationConstants.CUSTOM_LOCATION;
    public City currentCity;
    // private City nextCity;

    private LatLng currentLocation;
    private LatLng nextLocation;

    private Quaternion initialRotation;
    // For storing a copy of the last known time to limit updates
    private double lastTime;

    // Calculated from data
    public float minMag { get; private set; }
    public float maxMag { get; private set; }

    // Scene-specific settings, set via SceneManagerComponent
    private bool colorByConstellation = true;
    private bool showConstellationConnections = true;
    private int maxStars = 10000;
    private float magnitudeScale = 0.5f;
    private float magnitudeThreshold = 4.5f;

    private bool showHorizonView = false;
    private float simulationTimeScale = 10f;
    private float radius = 50;

    private SimulationManager manager;

    [SerializeField]
    private double lst;
    [SerializeField]
    private double julianDate;
    [SerializeField]
    private double longitudeTimeOffset;
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

    bool shouldUpdate = false;

    void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        manager = SimulationManager.GetInstance();
        // Need this so the network UI persists across scenes

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        SimulationEvents.GetInstance().LocationSelected.AddListener(handleSelectNewLocation);
    }
    private void OnDisable()
    {
        SimulationEvents.GetInstance().LocationSelected.RemoveListener(handleSelectNewLocation);
    }

    void OnSceneUnloaded(Scene scene)
    {
        this.isReady = false;
    }

    // This is where we set the scene parameters
    public void SetSceneParameters(int maxStars, float magnitudeScale, float magnitudeThreshold,
        bool showHorizonView, float simulationTimeScale, float radius, bool colorByConstellation,
        bool showConstellationConnections, Material starMaterial)
    {
        this.maxStars = maxStars;
        this.magnitudeScale = magnitudeScale;
        this.magnitudeThreshold = magnitudeThreshold;
        this.showHorizonView = showHorizonView;
        this.simulationTimeScale = simulationTimeScale;
        this.radius = radius;
        this.colorByConstellation = colorByConstellation;
        this.showConstellationConnections = showConstellationConnections;
        this.starMaterial = starMaterial;
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
            allStarComponents = new Dictionary<string, StarComponent>(); 
        }
        if (cityData != null)
        {
            allCities = DataImport.ImportCityData(cityData.text);
            Debug.Log(allCities.Count + " cities imported");
            cities = allCities.Select(c => c.Name).ToList();
            if (string.IsNullOrEmpty(startCity))
            {
                startCity = "Boston";
            }
            currentCity = allCities.Where(c => c.Name == startCity).FirstOrDefault();
            startCity = currentCity.Name;
            // changes to nextLocation trigger a change to Celestial Sphere orientation in Horizon view
            // and are captured on the SimulationEvents.LocationSelected event handler
            currentLocation = new LatLng {Latitude = currentCity.Lat, Longitude = currentCity.Lng};
            nextLocation = currentLocation;
        }
        if (constellationConnectionData != null)
        {
            allConstellationConnections = DataImport.ImportConstellationConnectionData(constellationConnectionData.text);
            Debug.Log(allConstellationConnections.Count + " connections imported");
        }

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
                    if (starMaterial)
                    {
                        newStar.GetComponent<Renderer>().material = starMaterial;
                    }
                    // Add star data, then position, scale and color
                    newStar.Init(constellationsController, dataStar, maxMag, magnitudeScale, SimulationManager.GetInstance().InitialRadius);

                    // Eventually store constellation color and observed star color separately
                    newStar.SetStarColor(constellationColor, Color.white);
                    // color by constellation
                    if (colorByConstellation == true)
                    {
                        Utils.SetObjectColor(newStar.gameObject, constellationColor);
                    }
                    constellation.highlightColor = constellationColor;

                    allStarComponents[dataStar.uniqueId] = newStar;
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
        if (manager == null) manager = SimulationManager.GetInstance();
        // Reset sphere
        isReady = false;
        this.transform.position = new Vector3(0, 0, 0f);
        this.transform.localScale = new Vector3(1, 1, 1) * manager.CurrentScaleFactor(radius);
        this.transform.rotation = Quaternion.identity;
        // userSpecifiedDateTime = manager.UseCustomSimulationTime;
        runSimulation = false;

        foreach (StarComponent starComponent in allStarComponents.Values)
        {
            Utils.SetObjectColor(starComponent.gameObject, colorByConstellation ? starComponent.constellationColor : Color.white);
            starComponent.SetStarScale(maxMag, magnitudeScale);
            if (starMaterial)
            {
                starComponent.GetComponent<Renderer>().material = starMaterial;
            }
        }
        
        // if we're in Horizon view, set location and date / time
        if (showHorizonView)
        {
            if (manager.UserHasSetLocation)
            {
                nextLocation = manager.LocalUserPin.Location;
                // Force an update once ready
                currentLocation = new LatLng();
            }
            positionNCP();
        }

        Debug.Log("updated");
        isReady = true;
    }

    void positionNCP()
    {
        // reset current rotation
        transform.rotation = Quaternion.identity;
        // NCP for selected latitude is due North, elevated at the same angle as latitude
        // this is our axis for siderial daily rotation.
        transform.rotation = Quaternion.Euler(90 - currentLocation.Latitude, 0, 0);
        initialRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        if (isReady && showHorizonView)
        {
            shouldUpdate = false;

            if (currentLocation != nextLocation)
            {
                currentLocation = nextLocation;
                positionNCP();
                shouldUpdate = true;
            }
           
            // allow change of time in all scenes - should work in Earth scene to switch seasons
            //double lst;
            if (manager == null) manager = SimulationManager.GetInstance();
            if (manager.UseCustomSimulationTime)
            {
                if (simulationTimeScale > 0 && runSimulation)
                {
                    manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddSeconds(Time.deltaTime * simulationTimeScale);
                }
            }
            
            julianDate = manager.CurrentSimulationTime.ToJulianDate();
            // Longitude relates to a time offset from UTC, 15 degrees per hour (24 hours in 360 degrees)
            longitudeTimeOffset = currentLocation.Longitude/15d;
            lst = manager.CurrentSimulationTime.ToSiderealTime() + longitudeTimeOffset;
            
            // Filter and only update positions if changed time / latitude
            if (lastTime != lst) shouldUpdate = true;

            if (shouldUpdate)
            {
                float rotationDueToUnityOrientation = 90;
                
                float siderealHoursPerDay = 23.9344696f;
                float fractionOfDay = (float)lst / siderealHoursPerDay;
                float planetRotation = (fractionOfDay * 360); // + longitudeRotation;
                
                // Start from initial rotation then rotate around for the current time and offset
                transform.rotation = initialRotation;
                transform.Rotate(0, (planetRotation + rotationDueToUnityOrientation), 0, Space.Self);
                // Set last timestamp so we only update when changed
                lastTime = lst;
            }
        }
    }
    
    void handleSelectNewLocation(string newCity)
    {
        Debug.Log("Got new location! " + newCity);
        SimulationManager manager = SimulationManager.GetInstance();
        
        if (!string.IsNullOrEmpty(newCity))
        {
            // If we have selected a new city by using the dropdown
            if (newCity != currentCity?.Name)
            {
                // verify a valid city was entered
                var matchedCity = allCities.Where(c => c.Name == newCity).First();
                if (matchedCity != null)
                {
                    // Raise the change event with the matching lat/lng to update UI
                    currentCity = matchedCity;
                    // check if this is a custom location
                    if (matchedCity.Name == SimulationConstants.CUSTOM_LOCATION)
                    {
                        // Grab the custom location from the simulation manager
                        nextLocation = manager.LocalUserPin.Location;
                        manager.Currentlocation = nextLocation;
                        manager.CurrentLocationName = SimulationConstants.CUSTOM_LOCATION;
                        SimulationEvents.GetInstance().LocationChanged.Invoke(manager.Currentlocation, SimulationConstants.CUSTOM_LOCATION);
                    }
                    else
                    {
                        // Set up the city lat lng
                        nextLocation = new LatLng{Latitude = matchedCity.Lat, Longitude = matchedCity.Lng};
                        manager.Currentlocation = nextLocation;
                        manager.CurrentLocationName = matchedCity.Name;
                        SimulationEvents.GetInstance().LocationChanged.Invoke(nextLocation, matchedCity.Name);
                    }
                    
                }
            }
        } 
    }
    public void SetMagnitudeThreshold(float newVal)
    {
        magnitudeThreshold = newVal;
        foreach (StarComponent starComponent in allStarComponents.Values)
        {
            starComponent.ShowStar(starComponent.starData.Mag < magnitudeThreshold);
        }
    }

    public void ToggleUserTime()
    {
        SimulationManager.GetInstance().UseCustomSimulationTime =
            !SimulationManager.GetInstance().UseCustomSimulationTime;
    }
    public void ToggleRunSimulation()
    {
        runSimulation = !runSimulation;
    }

    public void SetSimulationTimeScale(float newVal)
    {
        simulationTimeScale = newVal;
    }
}
