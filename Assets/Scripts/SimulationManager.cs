using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// This class is the manager Singleton, and contains specific references to application-level objects
public class SimulationManager
{
    // can't use constructor, guaranteed singleton
    protected SimulationManager() {
        LocalPlayer = new Player(new UserRecord() );
        CCDebug.Log($"User has been created {LocalPlayer.PlayerUserRecord.Username}", LogLevel.Info, LogMessageCategory.All);
    }
    private static SimulationManager instance;

    public static SimulationManager GetInstance()
    {
        return instance ?? (instance = new SimulationManager());
    }

    // A decent random generator
    public System.Random rng = new System.Random();

    public string[] Scenes {
        get {
#if UNITY_STANDALONE
            return SimulationConstants.SCENES_PC;
#elif UNITY_IOS
            return SimulationConstants.SCENES_AR;
#elif UNITY_ANDROID
            return SimulationConstants.SCENES_VR;
#else
            // Catch-all default
            return SimulationConstants.SCENES_PC;
#endif
        }
    }
    
    public string[] AnimalNames;
    public List<string> ColorNames = new List<string>();
    public List<Color> ColorValues = new List<Color>();

    public NetworkController NetworkControllerComponent;
    public GameObject InteractionControllerObject;
    public ServerRecord server = null;
    private GameObject celestialSphereObject;

    public GameObject CelestialSphereObject
    {
        get { return celestialSphereObject; }
        set
        {
            celestialSphereObject = value;
            DataControllerComponent = celestialSphereObject.GetComponent<DataController>();
            MarkersControllerComponent = celestialSphereObject.GetComponentInChildren<MarkersController>();
            ConstellationsControllerComponent = celestialSphereObject.GetComponentInChildren<ConstellationsController>();
        }
    }

    public DataController DataControllerComponent { get; private set; }
    public MarkersController MarkersControllerComponent { get; private set; }
    public ConstellationsController ConstellationsControllerComponent { get; private set; }

    public bool IsReady = false;

    public UserRecord LocalPlayerRecord
    {
        get { return localPlayer.PlayerUserRecord; }
    }

    public Color LocalPlayerColor {
        get { return LocalPlayerRecord.color; }
    }

    public string LocalUsername
    {
        get { return LocalPlayerRecord.Username; }
    }

    private Player localPlayer;

    public Player LocalPlayer
    {
        get { return localPlayer; }
        set
        {
            localPlayer = value;
        }
    }

    public Vector3 LocalPlayerLookDirection
    {
        get { return LocalPlayer.CameraDirection; }
        set { LocalPlayer.CameraDirection = value; }
    }
    
    public Color HorizonGroundColor = Color.green;

    // initial setup scale
    public readonly float InitialRadius = 100;
    // Scene Radius will be set in DataController, but we can keep a reference here for lookups elsewhere
    public float SceneRadius = 100;
    public float CurrentScaleFactor(float sceneRadius)
    {
        SceneRadius = sceneRadius;
        return sceneRadius / InitialRadius;
    }
    // Movement synchronization throttled for Heroku/Mongo
    public float MovementSendInterval = 1.0f;
    
    public StarComponent CurrentlySelectedStar;

    /// <summary>
    /// A shortcut to LocalPlayer.Pin.SelectedDateTime
    /// </summary>
    public DateTime CurrentSimulationTime
    {
        get { return LocalPlayer.Pin.SelectedDateTime; }
        set
        {
            Pushpin p  = new Pushpin(value, localPlayer.Pin.Location, localPlayer.Pin.LocationName);
            LocalPlayer.Pin = p;
        }
    }

    /// <summary>
    /// A shortcut to LocalPlayer.Pin.Location
    /// </summary>
    public LatLng CurrentLatLng
    {
        get { return LocalPlayer.Pin.Location; }
    }

    /// <summary>
    /// A shortcut to LocalPlayer.Pin.LocationName
    /// </summary>
    public string CurrentLocationName
    {
        get { return LocalPlayer.Pin.LocationName; }
    }

    /// <summary>
    /// A shortcut to LocalPlayer.Pin
    /// </summary>
    public Pushpin LocalPlayerPin
    {
        get { return LocalPlayer.Pin; }
    }

    /// <summary>
    /// This should be the only way to update the current time, the location latlng and the location name.
    /// </summary>
    /// <param name="pNext">Next pushpin</param>
    public void JumpToPin(Pushpin pNext)
    {
        Pushpin newPin = new Pushpin(pNext.SelectedDateTime, pNext.Location, pNext.LocationName);
        localPlayer.Pin = newPin;
    }
    private Dictionary<string, Player> remotePlayers = new Dictionary<string, Player>();
    
    public void AddOrUpdateRemotePlayer(string playerName)
    {
        if (!remotePlayers.ContainsKey(playerName))
        {
            Player p = new Player(playerName);
            remotePlayers.Add(playerName, p);
        }
    }
    public Player GetRemotePlayer(string playerName)
    {
        if (!remotePlayers.ContainsKey(playerName))
        {
            remotePlayers.Add(playerName, new Player(playerName));
        }
        return remotePlayers[playerName];
    }

    public float GetRelativeMagnitude(float starMagnitude)
    {
        //float min = DataControllerComponent.minMag;
        float max = DataControllerComponent.maxMag + Mathf.Abs(DataControllerComponent.minMag);
        return max - (starMagnitude + Mathf.Abs(DataControllerComponent.minMag));
    }

    private string groupName;

    public string GroupName
    {
        get
        {
            if (!string.IsNullOrEmpty(groupName)) return groupName;
            else{
                if (!string.IsNullOrEmpty(UserRecord.UserGroupFromPrefs()))
                {
                    groupName = UserRecord.UserGroupFromPrefs();
                    return groupName;
                }
                else
                {
                    return "unknown";
                }
            }
        }
    } 
    public Pushpin CrashSiteForGroup;
    public List<Pushpin> LocalUserSnapshots = new List<Pushpin>();
}