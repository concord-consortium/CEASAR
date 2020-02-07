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
    private static CCLogger instance;

    // Singleton Access
    public static CCLogger GetInstance()
    {
        return instance ?? (instance = new CCLogger());
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
        CCLogger logger = CCLogger.GetInstance();
        logger.RemoveEventListeners();
        Application.quitting -= Quit;
        Debug.Log("Logger Events removed.");
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
    }

    // Add Simulation state to the Logging event for context.
    private void UpdateContext()
    {
        SimulationManager manager = SimulationManager.GetInstance();
        time = getTimeMillies();
        scene = getSceneName();
        platformName = Application.platform.ToString();
        username = PlayerPrefs.GetString(USERNAME_PREF_KEY);
        groupName = PlayerPrefs.GetString(USER_GROUP_PREF_KEY);
        crashSite = UserRecord.GroupPins[groupName] != null
            ? UserRecord.GroupPins[groupName].ToString()
            : "";
        lastLocation = manager.LocalUserPin.Location.ToString();
        lastHeading = manager.LocalPlayerLookDirection.ToString();
        lastCompassHeading = Utils.CalcCompassOrdinal(manager.LocalPlayerLookDirection.y);
        lastSimulationTime = manager.CurrentSimulationTime.ToString();
        activity = CalcActivityName();


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
        SimulationEvents eventDispatcher = SimulationEvents.GetInstance();
        eventDispatcher.LocationSelected.AddListener(LocationSelected);
        // eventDispatcher.LocationChanged.AddListener(LocationChanged); 
        eventDispatcher.AnnotationAdded.AddListener(AnnotationAdded);
        eventDispatcher.AnnotationDeleted.AddListener(AnnotationDeleted);
        eventDispatcher.AnnotationClear.AddListener(AnnotationClear);
        eventDispatcher.PushPinSelected.AddListener(PushPinSelected);
        eventDispatcher.PushPinUpdated.AddListener(PushPinUpdated);
        eventDispatcher.DrawMode.AddListener(DrawMode);
        eventDispatcher.SimulationTimeChanged.AddListener(SimulationTimeChanged);
    }

    // Remove all listeners:
    private void RemoveEventListeners()
    {
        SimulationEvents eventDispatcher = SimulationEvents.GetInstance();
        eventDispatcher.LocationSelected.RemoveListener(LocationSelected);
        // eventDispatcher.LocationChanged.RemoveListener(LocationChanged);
        eventDispatcher.AnnotationAdded.RemoveListener(AnnotationAdded);
        eventDispatcher.AnnotationDeleted.RemoveListener(AnnotationDeleted);
        eventDispatcher.AnnotationClear.RemoveListener(AnnotationClear);
        eventDispatcher.PushPinSelected.RemoveListener(PushPinSelected);
        eventDispatcher.PushPinUpdated.RemoveListener(PushPinUpdated);
        eventDispatcher.DrawMode.RemoveListener(DrawMode);
        eventDispatcher.SimulationTimeChanged.RemoveListener(SimulationTimeChanged);
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
    private void AnnotationAdded(Vector3 position, Quaternion rotation, Vector3 scale, string name)
    {
        string msg = $"Name:{name}, P:{position.ToString()}, R:{rotation.ToString()}, S:{scale.ToString()}";
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
        DateTime currentTime = SimulationManager.GetInstance().CurrentSimulationTime;
        string msg = $"SimulationTime:{currentTime.ToString()}";
        _logAsync(LOG_EVENT_SIM_TIME_CHANGED, msg);
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
            Debug.LogError("Failed to send log to log server.");
            Debug.LogError(jsonLogMessage);
            Debug.LogError(e);
        }
    }


    private async Task _logAsync(string eventType, string msg)
    {
        @event = eventType;
        event_value = msg;
        message = msg;
        UpdateContext();
        bool devMode = SimulationManager.GetInstance().server == ServerList.Local;
        string json = JsonUtility.ToJson(this, true);

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
            Debug.Log("Logging: " + eventType + "-- " + msg);
        #endif

        #pragma warning disable CS4014
        // This call is not awaited, execution continues before the call is completed.
        // So as to not stall or interupt play while transmitting log messages.
        GetInstance()._logAsync(eventType, msg);
        #pragma warning restore CS4014
        
    }
    #endregion Public Interface
    /********* End Public interface *****************************************/
}
