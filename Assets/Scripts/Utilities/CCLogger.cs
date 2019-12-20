using System;
using UnityEngine.Networking;
using UnityEngine;

[Serializable]

public class LogItem
{
    public string application = "CEASAR";
    public string activity = "activity";
    public string scene = "scene";
    public double time = 0; // epoc millies
    // TODO: event_name should just be 'event' which is a reserved word …
    // CC Log Manager wants a JSON field "event"....
    public string event_name = "test event";
    public string event_value = "test value";
    public string platformName = "unknown";
    public string userName = "unknown";
    public string message = "";
}

public class CCLogger
{

    private static CCLogger instance;
    LogItem item;
    protected CCLogger() {
        item = new LogItem();
    }

    public static CCLogger GetInstance()
    {
        return instance ?? (instance = new CCLogger());
    }

    public async System.Threading.Tasks.Task logAsync()
    {
        TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        double millies = t.TotalMilliseconds;
        item.time = millies;
        item.application = "CEASAR";
        item.platformName = Application.platform.ToString();
        item.userName = PlayerPrefs.GetString("username");

        string json = JsonUtility.ToJson(item);
        string url = "http://cc-log-manager.herokuapp.com/api/logs";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);
    }

    public static void logDisconnect(string msg="")
    {
        CCLogger logger = GetInstance();
        logger.item.event_name = "Disconnect";
        logger.item.event_value = "Disconnect";
        logger.item.message = msg;
        logger.logAsync();
    }

    public static void logConnect(string msg = "")
    {
        CCLogger logger = GetInstance();
        logger.item.event_name = "Connect";
        logger.item.event_value = "Connect";
        logger.item.message = msg;
        logger.logAsync();
    }

    public static void logSetUsername(string msg = "")
    {
        CCLogger logger = GetInstance();
        logger.item.event_name = "setUserName";
        logger.item.event_value = "setUserName";
        logger.item.message = msg;
        logger.logAsync();
    }

    public static void logSceneLoaded(string msg="")
    {
        CCLogger logger = GetInstance();
        logger.item.event_name = "setUserName";
        logger.item.event_value = "setUserName";
        logger.item.message = msg;
        logger.logAsync();
    }

}
