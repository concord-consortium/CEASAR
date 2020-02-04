using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

[Serializable]
public class LogContext
{
    public const string APP_NAME = "CEASAR";
    public string application = APP_NAME;
    public string activity = "activity";
    public string scene = "scene";
    public double time = 0; // epoc millies
    public string event_value = "test value";
    public string platformName = "unknown";
    public string username = "unknown";
    public string groupName = "unknown";
    public string message = "";
    public LatLng lastLocation = new LatLng();
    public DateTime lastSimulationTime = new DateTime();
    public Vector3 lastHeading = new Vector3();

    // event is a reserved word, but we need to send it as JSON.
    // see: https://stackoverflow.com/questions/2787716/use-the-long-reserved-word-as-a-variable-name-in-c-sharp
    public string @event = "nothing";
    public LogContext()
    {
        time = getTimeMillies();
        application = APP_NAME;
        scene = getSceneName();
        platformName = Application.platform.ToString();
        username = PlayerPrefs.GetString(SimulationConstants.USERNAME_PREF_KEY);
        groupName = PlayerPrefs.GetString(SimulationConstants.USER_GROUP_PREF_KEY);
    }

    private static double getTimeMillies()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return t.TotalMilliseconds;
    }

    private static string getSceneName()
    {
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return scene.name;
    }
}

public class CCLogger
{
    public static readonly string LOGINGURL = "http://cc-log-manager.herokuapp.com/api/logs";
    public static readonly string EVENT_DISCONNECT = "Disconnect";
    public static readonly string EVENT_CONNECT = "Connect";
    public static readonly string EVENT_USERNAME = "Set username";
    public static readonly string EVENT_SCENE = "Scene loaded";
    public static readonly string EVENT_PLAYER_MOVE = "Player Move";
    public static readonly string EVENT_ADD_INTERACTION = "Interaction marker Added";
    public static readonly string APP_NAME = "CEASAR";


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

    public LatLng lastLocation;
    public DateTime lastSimulationTime;
    public Vector3 lastHeading;
    public string lastLocationName;
    public string selectedObject;
    public string selectedStar;

    private static CCLogger instance;

    public static CCLogger GetInstance()
    {
        return instance ?? (instance = new CCLogger());
    }

    private CCLogger()
    {
        InitializeContext();
        UpdateContext();
        RegisterEventListeners();
    }

    // Initialize local variables to default values.
    // Will be updated with real data later after events start coming in.
    private void InitializeContext()
    {
        application = APP_NAME;
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
        lastLocation = new LatLng();
        lastSimulationTime = new DateTime();
        lastHeading = new Vector3();
    }

    private void UpdateContext()
    {
        SimulationManager manager = SimulationManager.GetInstance();
        time = getTimeMillies();
        scene = getSceneName();
        platformName = Application.platform.ToString();
        username = PlayerPrefs.GetString(SimulationConstants.USERNAME_PREF_KEY);
        groupName = PlayerPrefs.GetString(SimulationConstants.USER_GROUP_PREF_KEY);
        lastLocation = manager.Currentlocation;
        lastSimulationTime = manager.CurrentSimulationTime;
        if (manager.CurrentLocationName != null && manager.CurrentLocationName.Length > 0)
        {
            lastLocationName = manager.CurrentLocationName;
        }
        if (manager.CelestialSphereObject != null)
        {
            selectedObject = manager.CelestialSphereObject.name;
        }
        else
        {
            selectedObject = "";
        }
        if (manager.CurrentlySelectedStar != null)
        {
            selectedStar = manager.CurrentlySelectedStar.name;
        }
        else
        {
            selectedObject = "";
        }

    }


    private void RegisterEventListeners()
    {
        SimulationEvents eventDispatcher = SimulationEvents.GetInstance();
        eventDispatcher.LocationSelected.AddListener(LocationSelected);
        eventDispatcher.LocationChanged.AddListener(LocationChanged);
        eventDispatcher.AnnotationAdded.AddListener(AnnotationAdded);
        eventDispatcher.AnnotationDeleted.AddListener(AnnotationDeleted);
        eventDispatcher.AnnotationClear.AddListener(AnnotationClear);
        eventDispatcher.PushPinSelected.AddListener(PushPinSelected);
        eventDispatcher.PushPinUpdated.AddListener(PushPinUpdated);
        eventDispatcher.DrawMode.AddListener(DrawMode);
        eventDispatcher.SimulationTimeChanged.AddListener(SimulationTimeChanged);
    }
    private void LocationSelected(string newLocation)
    {
        _logAsync("UI Location Selected", newLocation);
    }


    private void LocationChanged(LatLng location, string place)
    {
        lastLocation = location;
        string locationString = $"{location.ToDisplayString()} ({place})";
        _logAsync("UI Location Changed", locationString);
    }

    // localPosition, localRotation, localScale, name 
    private void AnnotationAdded(Vector3 position, Quaternion rotation, Vector3 scale, string name)
    {
        string msg = $"Name:{name}, P:{position.ToString()}, R:{rotation.ToString()}, S:{scale.ToString()}";
        _logAsync("Annotation Added", msg);
    }

    private void AnnotationDeleted(string name)
    {
        _logAsync("Annotation Deleted", name);
    }

    private void AnnotationClear(string name)
    {
        _logAsync("Annotation Clear", name);
    }

    private void PushPinSelected(LatLng location, DateTime date)
    {
        lastLocation = location;
        lastSimulationTime = date;
        string msg = $"Location: {location.ToDisplayString()}, date:{date.ToString()}";
        _logAsync("PushPin Selected", msg);
    }

    // Currentlocation, CurrentSimulationTime, cameraRotation
    private void PushPinUpdated(LatLng location, DateTime date, Vector3 rotation)
    {
        lastLocation = location;
        lastSimulationTime = date;
        lastHeading = rotation;
        string msg = $"Location:{location.ToDisplayString()}, date:{date.ToString()}, ";
        _logAsync("PushPin Updated", msg);
    }

    private void DrawMode(bool isDrawing)
    {
        string msg = $"isDrawing:{isDrawing.ToString()}";
        _logAsync("DrawMode Changed", msg);
    }

    public void SimulationTimeChanged() {
        DateTime currentTime = SimulationManager.GetInstance().CurrentSimulationTime;
        string msg = $"SimulationTime:{currentTime.ToString()}";
        _logAsync("Simulation Time Changed", msg);
    }



    private static double getTimeMillies()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        return t.TotalMilliseconds;
    }

    private static string getSceneName()
    {
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return scene.name;
    }

    private async Task postLog(string jsonLogMessage)
    {
        try
        {
            var request = new UnityWebRequest(LOGINGURL, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonLogMessage);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            await request.SendWebRequest();
            Debug.Log("Logger returned http status code: " + request.responseCode);
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
        event_value = eventType;
        message = msg;
        UpdateContext();
        bool devMode = SimulationManager.GetInstance().server == ServerList.Local;
        string json = JsonUtility.ToJson(this, true);

        Debug.Log(json);

        // Only log if we're on the network to reduce clutter
        if (!devMode)
        {
            await postLog(json);
        }
        // Otherwise dump it to console for inspection.
        else
        {
            Debug.Log("<FAKE CC_LOGER>--------------------");
            Debug.Log(json);
            Debug.Log("-------------------</FAKE CC_LOGER>");
        }
        
    }

    public static void Log(string eventType, string msg)
    {
        Debug.Log("Logging: " + eventType + "-- " + msg);
#pragma warning disable CS4014
        // This call is not awaited, execution continues before the call is completed.
        // So as to not stall or interupt play while transmitting log messages.
        GetInstance()._logAsync(eventType, msg);
#pragma warning restore CS4014
        
    }
}
