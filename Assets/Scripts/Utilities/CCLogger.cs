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
    public string message = "";
    // event is a reserved word, but we need to send it as JSON.
    // see: https://stackoverflow.com/questions/2787716/use-the-long-reserved-word-as-a-variable-name-in-c-sharp
    public string @event = "nothing";
    public LogContext()
    {
        time = getTimeMillies();
        application = APP_NAME;
        scene = getSceneName();
        platformName = Application.platform.ToString();
        username = PlayerPrefs.GetString(NetworkController.PLAYER_PREFS_NAME_KEY);
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
    public const string LOGINGURL = "http://cc-log-manager.herokuapp.com/api/logs";
    public const string EVENT_DISCONNECT = "Disconnect";
    public const string EVENT_CONNECT = "Connect";
    public const string EVENT_USERNAME = "Set username";
    public const string EVENT_SCENE = "Scene loaded";
    public const string EVENT_PLAYER_MOVE = "Player Move";
    public const string EVENT_ADD_MARKER = "Marker Added";
    private static CCLogger instance;

    public static CCLogger GetInstance()
    {
        return instance ?? (instance = new CCLogger());
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
        LogContext context = new LogContext();
        context.@event = eventType;
        context.event_value = eventType;
        context.message = msg;
        string json = JsonUtility.ToJson(context);
        await postLog(json);
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
