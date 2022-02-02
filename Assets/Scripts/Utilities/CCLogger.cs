using System;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using static SimulationConstants;

[Serializable]
public class CCLogger
{

    public string application;
    public string activity;
    public string scene;
    public double time; // epoc millies
    public string platformName;
    public string username;
    public string groupName;
    public string message;

    // event is a reserved word, but we need to send it as JSON.
    // see: https://stackoverflow.com/questions/2787716/use-the-long-reserved-word-as-a-variable-name-in-c-sharp
    public string @event = "nothing";
    public string event_value;

    public string lastLocation;
    public string lastSimulationTime;
    public string lastHeading;
    public string lastCompassHeading;
    public string lastLocationName;
    public string selectedObject;
    public string selectedStar;
    public string crashSite;
    public string northPinDirection;
    private static CCLogger instance;

    // Singleton Access
    public static CCLogger Instance
    {
        get { return instance ?? (instance = new CCLogger()); }
    }

    // Private Constructor
    private CCLogger()
    {
        InitializeContext();
        UpdateContext();
        AddEventListeners();
    }

    // Hook to make sure our other listeners are removed
    static void Quit()
    {
        CCLogger logger = CCLogger.Instance;
        logger.RemoveEventListeners();
        Application.quitting -= Quit;
        CCDebug.Log("Logger Events removed.");
    }


    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        // Register our quit hook, which removes event listeners
        Application.quitting += Quit;
    }

    // Initialize local variables to default values.
    // Will be updated with real data later after events start coming in.
    private void InitializeContext()
    {
        application = LOG_APP_NAME;
        activity = "activity";
        scene = "scene";
        platformName = "unknown";
        username = "unknown";
        groupName = "unknown";
        message = "";
        event_value = "";
        lastLocationName = "";
        selectedObject = "";
        selectedStar = "";
        lastLocation = "";
        lastSimulationTime = "";
        lastHeading = "";
        lastCompassHeading = "";
        northPinDirection = "";
    }

    // Add Simulation state to the Logging event for context.
    private void UpdateContext()
    {
        SimulationManager manager = SimulationManager.Instance;
        time = getTimeMillies();
        scene = getSceneName();
        platformName = Application.platform.ToString();
        username = PlayerPrefs.GetString(USERNAME_PREF_KEY);
        groupName = PlayerPrefs.GetString(USER_GROUP_PREF_KEY);
        crashSite = UserRecord.GroupPins[groupName] != null
            ? UserRecord.GroupPins[groupName].ToString()
            : "";
        lastLocation = manager.LocalPlayerPin.Location.ToString();
        lastHeading = manager.LocalPlayerLookDirection.ToString();
        lastCompassHeading = Utils.CalcCompassOrdinal(manager.LocalPlayerLookDirection.y);
        lastSimulationTime = manager.CurrentSimulationTime.ToString();
        activity = CalcActivityName();
        northPinDirection = manager.PlayerNorthPinDirection.ToString();

        if (manager.CurrentLocationName != null && manager.CurrentLocationName.Length > 0)
        {
            lastLocationName = manager.CurrentLocationName;
        }
        if (manager.CurrentlySelectedStar != null)
        {
            selectedStar = manager.CurrentlySelectedStar.starData.ProperName;
            selectedObject = manager.CurrentlySelectedStar.starData.ConstellationFullName;
        }
        else
        {
            selectedObject = "";
            selectedStar = "";
        }
    }

    private string CalcActivityName()
    {
        return $"{groupName}, {scene}, {platformName}";
    }


    // Listen for game events, dispatch to log events:
    private void AddEventListeners()
    {
        SimulationEvents eventDispatcher = SimulationEvents.Instance;
        eventDispatcher.LocationSelected.AddListener(LocationSelected);
        // eventDispatcher.LocationChanged.AddListener(LocationChanged);
        eventDispatcher.AnnotationAdded.AddListener(AnnotationAdded);
        eventDispatcher.AnnotationDeleted.AddListener(AnnotationDeleted);
        eventDispatcher.AnnotationClear.AddListener(AnnotationClear);
        eventDispatcher.PushPinSelected.AddListener(PushPinSelected);
        eventDispatcher.PushPinUpdated.AddListener(PushPinUpdated);
        eventDispatcher.PlayerNorthPin.AddListener(PlayerNorthPinUpdated);
        eventDispatcher.DrawMode.AddListener(DrawMode);
        eventDispatcher.SimulationTimeChanged.AddListener(SimulationTimeChanged);
        eventDispatcher.SnapshotCreated.AddListener(SnapshotCreated);
        eventDispatcher.SnapshotLoaded.AddListener(SnapshotLoaded);
        eventDispatcher.SnapshotDeleted.AddListener(SnapshotDeleted);
        eventDispatcher.StarSelected.AddListener(StarSelected);
        eventDispatcher.SunSelected.AddListener(SunSelected);
        eventDispatcher.MoonSelected.AddListener(MoonSelected);
        eventDispatcher.ConstellationSelected.AddListener(ConstellationSelected);
    }

    // Remove all listeners:
    private void RemoveEventListeners()
    {
        SimulationEvents eventDispatcher = SimulationEvents.Instance;
        eventDispatcher.LocationSelected.RemoveListener(LocationSelected);
        // eventDispatcher.LocationChanged.RemoveListener(LocationChanged);
        eventDispatcher.AnnotationAdded.RemoveListener(AnnotationAdded);
        eventDispatcher.AnnotationDeleted.RemoveListener(AnnotationDeleted);
        eventDispatcher.AnnotationClear.RemoveListener(AnnotationClear);
        eventDispatcher.PushPinSelected.RemoveListener(PushPinSelected);
        eventDispatcher.PushPinUpdated.RemoveListener(PushPinUpdated);
        eventDispatcher.PlayerNorthPin.RemoveListener(PlayerNorthPinUpdated);
        eventDispatcher.DrawMode.RemoveListener(DrawMode);
        eventDispatcher.SimulationTimeChanged.RemoveListener(SimulationTimeChanged);
        eventDispatcher.SnapshotCreated.RemoveListener(SnapshotCreated);
        eventDispatcher.SnapshotLoaded.RemoveListener(SnapshotLoaded);
        eventDispatcher.SnapshotDeleted.RemoveListener(SnapshotDeleted);
        eventDispatcher.StarSelected.RemoveListener(StarSelected);
        eventDispatcher.SunSelected.RemoveListener(SunSelected);
        eventDispatcher.MoonSelected.RemoveListener(MoonSelected);
        eventDispatcher.ConstellationSelected.RemoveListener(ConstellationSelected);
    }

    /********************* EVENT DISPATCHING SECTION ************************/
    #region Event Dispatching:

    private void LocationSelected(string newLocation)
    {
        _logAsync(LOG_EVENT_LOCATION_SELECTED, newLocation);
    }


    private void LocationChanged(LatLng location, string place)
    {
        lastLocation = location.ToDisplayString();
        string locationString = $"{location.ToDisplayString()} ({place})";
        _logAsync(LOG_EVENT_LOCATION_CHANGED, locationString);
    }

    // localPosition, localRotation, localScale, name
    private void AnnotationAdded(Vector3 startPos, Vector3 endPos, Vector3 rotation, string name)
    {
        string msg = $"Name:{name}, StartP:{startPos.ToString()}, EndP:{endPos.ToString()} Rot:{rotation.ToString()}";
        _logAsync(LOG_EVENT_ANNOTATION_ADDED, msg);
    }

    private void AnnotationDeleted(string name)
    {
        _logAsync(LOG_EVENT_ANNOTATION_DELETED, name);
    }

    private void AnnotationClear(string name)
    {
        _logAsync(LOG_EVENT_ANNOTATION_CLEARED, name);
    }

    private void PushPinSelected(Pushpin pin)
    {
        lastLocation = pin.Location.ToString();
        lastSimulationTime = pin.SelectedDateTime.ToString();
        string msg = $"Pushpin Selected: {pin.ToString()}";
        _logAsync(LOG_EVENT_PUSHPIN_SELECTED, msg);
    }

    // Currentlocation, CurrentSimulationTime, cameraRotation
    private void PushPinUpdated(Pushpin pin, Vector3 rotation)
    {
        lastLocation = pin.Location.ToDisplayString();
        lastSimulationTime = pin.SelectedDateTime.ToString();
        lastHeading = rotation.ToString();
        string msg = $"Pushpin Updated: {pin.ToString()}, {rotation.ToString()}";
        _logAsync(LOG_EVENT_PUSHPIN_UPDATED, msg);
    }

    private void PlayerNorthPinUpdated(float pinDirection)
    {
        string msg = $"Positioned the North Pin: {pinDirection.ToString()}";
        _logAsync(LOG_EVENT_NORTHPIN_UPDATED, msg);
    }

    private void DrawMode(bool isDrawing)
    {
        if(isDrawing)
        {
          _logAsync(LOG_EVENT_DRAWMODE_STARTED, "DrawMode Started");
        }
        else
        {
            _logAsync(LOG_EVENT_DRAWMODE_ENDED, "DrawMode Ended");
        }
    }

    private void SimulationTimeChanged() {
        DateTime currentTime = SimulationManager.Instance.CurrentSimulationTime;
        string msg = $"SimulationTime:{currentTime.ToString()}";
        _logAsync(LOG_EVENT_SIM_TIME_CHANGED, msg);
    }

    private void SnapshotCreated(Pushpin pin)
    {
        lastLocation = pin.Location.ToDisplayString();
        lastSimulationTime = pin.SelectedDateTime.ToString();
        string msg = $"Snapshot Created: {pin.ToString()}";
        _logAsync(LOG_EVENT_SNAPSHOT_CREATED, msg);
    }
    private void SnapshotLoaded(Pushpin pin)
    {
        lastLocation = pin.Location.ToDisplayString();
        lastSimulationTime = pin.SelectedDateTime.ToString();
        string msg = $"Snapshot Loaded: {pin.ToString()}";
        _logAsync(LOG_EVENT_SNAPSHOT_LOADED, msg);
    }
    private void SnapshotDeleted(Pushpin pin)
    {
        lastLocation = pin.Location.ToDisplayString();
        lastSimulationTime = pin.SelectedDateTime.ToString();
        string msg = $"Snapshot Deleted: {pin.ToString()}";
        _logAsync(LOG_EVENT_SNAPSHOT_DELETED, msg);
    }

    private void StarSelected(Star selectedStarData, string playerName, Color playerColor) {
        if (selectedStarData == null) return;
        string starName = selectedStarData.ProperName.Length > 0 ? selectedStarData.ProperName : "N/A";
        string starHip = selectedStarData.Hipparcos.ToString();
        string msg = $"Name:{starName}, Hipparcos#:{starHip}, Player:{playerName}";
        _logAsync(LOG_EVENT_STAR_SELECTED, msg);
    }
    private void SunSelected(bool selected) {
        string msg = "Sun selected";
        _logAsync(LOG_EVENT_SUN_SELECTED, msg);
    }
    private void MoonSelected(bool selected) {
        string msg = "Moon selected";
        _logAsync(LOG_EVENT_MOON_SELECTED, msg);
    }
    private void ConstellationSelected(string constellation) {
        _logAsync(LOG_EVENT_CONSTELLATION_SELECTED, constellation);
    }

    #endregion Event Dispatching:
    /********************* END EVENT DISPATCHING SECTION ********************/


    private static double getTimeMillies()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return t.TotalMilliseconds;
    }

    private static string getSceneName()
    {
        Scene scene = SceneManager.GetActiveScene();
        return scene.name;
    }

    private async Task postLog(string jsonLogMessage)
    {
        try
        {
            var request = new UnityWebRequest(LOG_URL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonLogMessage);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
        } catch (Exception e)
        {
            CCDebug.LogError("Failed to send log to log server.");
            CCDebug.LogError(jsonLogMessage);
            CCDebug.LogError(e);
        }
    }


    private async Task _logAsync(string eventType, string msg)
    {
        @event = eventType;
        event_value = msg;
        message = msg;
        UpdateContext();
        bool devMode = SimulationManager.Instance.server == ServerList.Local;
        string json = JsonUtility.ToJson(this, true);
        CCDebug.Log(msg, LogLevel.Info, LogMessageCategory.EventLog);
        // Only send to log manager if we're on the network to reduce clutter
        if (!devMode)
        {
            await postLog(json);
        }
    }

    /********* Public Interface *********************************************/
    #region Public Interface:

    public static void Log(string eventType, string msg)
    {
        #if UNITY_EDITOR
            CCDebug.Log("Logging: " + eventType + "-- " + msg, LogLevel.Verbose, LogMessageCategory.All);
        #endif

        #pragma warning disable CS4014
        // This call is not awaited, execution continues before the call is completed.
        // So as to not stall or interupt play while transmitting log messages.
        Instance._logAsync(eventType, msg);
        #pragma warning restore CS4014

    }
    #endregion Public Interface
    /********* End Public interface *****************************************/
}
