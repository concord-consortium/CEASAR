using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;
using static SimulationConstants;

/// <summary>
/// This class is for handling setting up the initial celestial sky objects and all the constellations.
/// Data about stars and constellations is stored in the DataManager.
/// Information about simulation parameters is stored in SimulationManager.
/// The celestial sky objects will rotate according to simulation settings.
/// </summary>
public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;
    public TextAsset constellationConnectionData;

    public StarComponent starPrefab;
    public Material starMaterial;

    public GameObject allConstellations;
    private ConstellationsController constellationsController;
    public Constellation constellationPrefab;
    private Dictionary<string, StarComponent> allStarComponents;

    public float MinMagnitudeValue = -2;
    public float MaxMagnitudeValue = 8;

    private float _magnitudeVisibilityIncrement = 0.5f;

    private float magnitudeThreshold = 4.5f;

    public float MagnitudeThreshold => magnitudeThreshold;

    public StarComponent GetStarById(string uniqueId)
    {
        return allStarComponents[uniqueId];
    }
    private bool runSimulation = false;

    public bool IsRunningSimulation
    {
        get { return runSimulation; }
    }

    private LatLng currentLocation;

    private Quaternion initialRotation;
    // For storing a copy of the last known time to limit updates
    private double lastTime;


    // Scene-specific settings, set via SceneManagerComponent
    private bool colorByConstellation = true;
    private bool showConstellationConnections = true;
    private int maxStars = 10000;
    private float magnitudeScale = 0.5f;

    private bool showHorizonView = false;
    private float simulationTimeScale = 10f;
    private float radius = 50;

    SimulationManager manager { get => SimulationManager.Instance; }

    private DataManager dataManager { get => DataManager.Instance; }

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

    bool shouldUpdate = false;

    void Awake()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        // Need this so the network UI persists across scenes
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        SimulationEvents.Instance.LocationSelected.AddListener(handleSelectNewLocation);
        SimulationEvents.Instance.PushPinSelected.AddListener(handlePinSelected);
    }
    private void OnDisable()
    {
        SimulationEvents.Instance.LocationSelected.RemoveListener(handleSelectNewLocation);
        SimulationEvents.Instance.PushPinSelected.RemoveListener(handlePinSelected);
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
        this.lastTime = 0;
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
            DataImport.ImportAllData(starData.text, maxStars, cityData.text, constellationConnectionData.text);
            CCDebug.Log(dataManager.Stars.Count + " stars imported");
            allStarComponents = new Dictionary<string, StarComponent>();
        }
        if (cityData != null)
        {
            CCDebug.Log(dataManager.Cities.Count + " cities imported");
            // changes to nextLocation trigger a change to Celestial Sphere orientation in Horizon view
            // and are captured on the SimulationEvents.LocationSelected event handler
            currentLocation = manager.CurrentLatLng;
        }
        if (constellationConnectionData != null)
        {
            CCDebug.Log(dataManager.Connections.Count + " connections imported");
        }

        int starCount = 0;
        if (starPrefab != null && dataManager.Stars != null && dataManager.Stars.Count > 0)
        {
            // get magnitudes and normalize between 1 and 5 to scale stars
            CCDebug.Log(dataManager.MinMag + " " + dataManager.MaxMag + " constellations:" + dataManager.ConstellationFullNames.Count);

            foreach (ConstellationNamePair constellationName in dataManager.ConstellationNames)
            {
                List<Star> starsInConstellation = dataManager.AllStarsInConstellation(constellationName.shortName);

                Constellation constellation = Instantiate(constellationPrefab, allConstellations.transform);
                constellation.name = constellationName.shortName.Trim() == "" ? "no-const" : constellationName.shortName;

                // Add constellation name information
                constellation.constellationNameAbbr = constellationName.shortName;
                constellation.constellationNameFull = constellationName.fullName;

                // Add connections
                foreach (ConstellationConnection conn in dataManager.Connections)
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
                        newStar.StarVisible.GetComponent<Renderer>().material = starMaterial;
                    }
                    // Add star data, then position, scale and color
                    newStar.Init(constellationsController, dataStar, dataManager.MaxMag, magnitudeScale, manager.InitialRadius);

                    constellation.highlightColor = constellationColor;

                    allStarComponents[dataStar.uniqueId] = newStar;
                    starCount++;

                    if (dataStar.Mag < MinMagnitudeValue) MinMagnitudeValue = dataStar.Mag;
                    if (dataStar.Mag > MaxMagnitudeValue) MaxMagnitudeValue = dataStar.Mag;

                    // show or hide based on magnitude threshold - note that magnitudes are
                    // a strange scale where lower numbers are brighter. The threshold is used to
                    // limit the number of stars shown by hiding stars that appear more dim (higher magnitude)
                    if (dataStar.Mag > magnitudeThreshold)
                    {
                        bool starIsConstellationConnection = DataManager.Instance.ConstellationConnectionStars.Contains(dataStar.Hipparcos);
                        newStar.ShowStar(starIsConstellationConnection || false);
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
        this.transform.localScale = new Vector3(1, 1, 1) * manager.CurrentScaleFactor(radius);
        this.transform.rotation = Quaternion.identity;
        // userSpecifiedDateTime = manager.UseCustomSimulationTime;
        runSimulation = false;

        foreach (StarComponent starComponent in allStarComponents.Values)
        {
            Utils.SetObjectColor(starComponent.StarVisible, colorByConstellation ? starComponent.constellationColor : starComponent.starColor);
            starComponent.SetStarScale(magnitudeScale);
            if (starMaterial)
            {
                starComponent.StarVisible.GetComponent<Renderer>().material = starMaterial;
            }
        }

        // if we're in Horizon view, set location and date / time
        if (showHorizonView)
        {
            // Force an update once ready
            currentLocation = new LatLng();
            positionNCP();
        }

        CCDebug.Log("Data controller updated", LogLevel.Verbose, LogMessageCategory.Event);
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

            if (currentLocation != manager.CurrentLatLng)
            {
                currentLocation = manager.CurrentLatLng;
                positionNCP();
                shouldUpdate = true;
            }

            // allow change of time in all scenes - should work in Earth scene to switch seasons
            //double lst;

            if (simulationTimeScale > 0 && runSimulation)
            {
                manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddSeconds(Time.deltaTime * simulationTimeScale);
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
        CCDebug.Log("Got new location! " + newCity, LogLevel.Info, LogMessageCategory.Event);

        if (!string.IsNullOrEmpty(newCity))
        {
            // verify a valid city was entered
            var matchedCities = dataManager.Cities.Where(c => c.Name == newCity);

            if (matchedCities.Count() > 0)
            {
                var matchedCity = matchedCities.First();
                // check if this is a custom location
                if (matchedCity.Name != SimulationConstants.CUSTOM_LOCATION)
                {
                    // PushPinSelected event will update manager and update next location
                    Pushpin pin = new Pushpin(manager.CurrentSimulationTime,
                        new LatLng {Latitude = matchedCity.Lat, Longitude = matchedCity.Lng}, matchedCity.Name);
                    manager.JumpToPin(pin);
                    // Update local listeners for UI and game object updates
                    SimulationEvents.Instance.PushPinSelected.Invoke(pin);

                    // broadcast the update to remote players
                    SimulationEvents.Instance.PushPinUpdated.Invoke(pin, manager.LocalPlayerLookDirection);
                }
                else
                {
                    manager.JumpToPin(manager.CrashSiteForGroup);

                    SimulationEvents.Instance.PushPinSelected.Invoke(manager.CrashSiteForGroup);
                    SimulationEvents.Instance.PushPinUpdated.Invoke(manager.CrashSiteForGroup,
                        manager.LocalPlayerLookDirection);
                }
            }
        }
    }

    void handlePinSelected(Pushpin pin)
    {
        // force a redraw next frame
        currentLocation = new LatLng();
    }
    public void SetMagnitudeThreshold(float newVal)
    {
        magnitudeThreshold = newVal;
        foreach (StarComponent starComponent in allStarComponents.Values)
        {
            bool starIsConstellationConnection = DataManager.Instance.ConstellationConnectionStars.Contains(starComponent.starData.Hipparcos);
            starComponent.ShowStar(starIsConstellationConnection || starComponent.starData.Mag < magnitudeThreshold);
        }
    }

    public void ShowMoreStars()
    {
        float newThreshold = magnitudeThreshold + _magnitudeVisibilityIncrement;
        if (newThreshold <= MaxMagnitudeValue)
        {
            SetMagnitudeThreshold(newThreshold);
            CCLogger.Log(LOG_EVENT_SHOW_MORE_STARS, "new threshold:" + newThreshold.ToString());
        }
    }

    public void ShowFewerStars()
    {
        float newThreshold = magnitudeThreshold - _magnitudeVisibilityIncrement;
        if (newThreshold >= ((MaxMagnitudeValue - MinMagnitudeValue) / 3))
        {
            SetMagnitudeThreshold(newThreshold);
            CCLogger.Log(LOG_EVENT_SHOW_FEWER_STARS, "new threshold:" + newThreshold.ToString());
        }
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
