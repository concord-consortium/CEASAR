using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;

public enum NetworkConnection { Local, Dev, Remote, None }

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    public const string PLAYER_PREFS_NAME_KEY = "CAESAR_USERNAME";
   
    public GameObject avatar;
    private Player localPlayer;
    private GameObject localPlayerAvatar;
    public GameObject interactionIndicator;

    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected ColyseusClient colyseusClient;

    protected IndexedDictionary<string, GameObject> remotePlayers = new IndexedDictionary<string, GameObject>();

    public bool autoConnect = false;
    public List<string> scenesWithAvatars;

    public NetworkConnection networkConnection = NetworkConnection.Remote;

    public NetworkUI networkUI;
    private NetworkConnection _selectedNetwork = NetworkConnection.None;

    SimulationManager manager;

    private bool _isConnected = false;
    public bool IsConnected
    {
        get { return colyseusClient != null && colyseusClient.IsConnected; }
    }

    public string ServerStatusMessage {
        set { networkUI.ConnectionStatusText = value; }
    }
    
 
    // Need this so the network UI persists across scenes
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        networkUI = FindObjectOfType<NetworkUI>();
        manager = SimulationManager.GetInstance();
        ensureUsername();
        networkUI.Username = manager.LocalUsername;
    }
    void Start()
    {
        colyseusClient = GetComponent<ColyseusClient>();

        if (autoConnect)
        {
            ConnectToServer();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SimulationEvents.GetInstance().AnnotationAdded.AddListener(BroadcastAnnotation);
        SimulationEvents.GetInstance().AnnotationDeleted.AddListener(BroadcastDeleteAnnotation);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        // instance of UI will have changed per scene
        networkUI = FindObjectOfType<NetworkUI>();
        ensureUsername();
        if (IsConnected && scenesWithAvatars.Contains(scene.name))
        {
            OnPlayerAdd(localPlayer);
            foreach (string p in remotePlayers.Keys)
            {
                Player remotePlayer = colyseusClient.GetPlayerById(p);
                OnPlayerAdd(remotePlayer);
            }
        }
        
        refreshUI();
        CCLogger.Log(CCLogger.EVENT_SCENE, "OnSceneLoaded: " + scene.name);
    }

    private void Update()
    {
        // only need to look out for connection status changes
        // then tell the UI to update so we get a visual cue
        if (_isConnected != IsConnected)
        {
            _isConnected = IsConnected;
            refreshUI();
        }
    }

    public void NetworkPanelToggled(bool active)
    {
        if (active)
        {
            refreshUI();
        }
    }
    void refreshUI()
    {
        // refresh connection status on hide/show and on scene change
        networkUI = FindObjectOfType<NetworkUI>();
        networkUI.ConnectButtonText = IsConnected ? "Disconnect" : "Connect";
        updatePlayerList();
    }


    public void SetNetworkAddress(NetworkConnection destination)
    {
        switch (destination)
        {
            case NetworkConnection.Local:
                networkUI.NetworkAddress = manager.LocalNetworkServer;
                break;
            case NetworkConnection.Dev:
                networkUI.NetworkAddress = manager.DevNetworkServer;
                break;
            case NetworkConnection.Remote:
                networkUI.NetworkAddress = manager.ProductionNetworkServer;
                break;
            default:
                networkUI.NetworkAddress = manager.ProductionNetworkServer;
                break;
        }

        _selectedNetwork = destination;
    }
    void ensureUsername()
    {
        if(!string.IsNullOrEmpty(manager.LocalUsername))
        {
            return;
        }
        string foundName = PlayerPrefs.GetString(PLAYER_PREFS_NAME_KEY);
        if (string.IsNullOrEmpty(foundName))
        {
            RandomizeUsername();
        }
        else
        {
            manager.LocalUsername = foundName;
            updateLocalAvatar();
        }
    }


    public void RandomizeUsername()
    {
        if (!IsConnected)
        {
            manager.GenerateUsername();
            localPlayerAvatar = GameObject.FindWithTag("LocalPlayerAvatar");
            updateLocalAvatar();
            PlayerPrefs.SetString(PLAYER_PREFS_NAME_KEY, manager.LocalUsername);
            networkUI.Username = manager.LocalUsername;
            CCLogger.Log(CCLogger.EVENT_USERNAME, "Username set " + manager.LocalUsername);
        }
    }

    public void ConnectToServer(string userDefinedEndpoint = null)
    {
        /*
         * Get Colyseus endpoint from InputField
         *  for localhost use ws://localhost:2567/        
         */
        if (!IsConnected)
        {
            ensureUsername();
            string _localEndpoint = manager.LocalNetworkServer;
            string _remoteEndpoint = manager.ProductionNetworkServer;
            string endpoint = string.Empty;

            if (string.IsNullOrEmpty(userDefinedEndpoint))
            {
                // no user interaction with the network address, work on some defaults
#if UNITY_EDITOR
                endpoint = _localEndpoint;
                if (_selectedNetwork == NetworkConnection.None) _selectedNetwork = NetworkConnection.Local;
#else
                endpoint = _remoteEndpoint;
                if (_selectedNetwork == NetworkConnection.None) _selectedNetwork = NetworkConnection.Remote;
#endif
            }
            else
            {
                endpoint = userDefinedEndpoint;
            }
            colyseusClient.ConnectToServer(endpoint, manager.LocalUsername);
            manager.NetworkStatus = _selectedNetwork;
            refreshUI();
            CCLogger.Log(CCLogger.EVENT_CONNECT, "connected");
        }
        else if (IsConnected)
        {
            Disconnect();
        }
        else
        {
            Debug.Log("Already disconnected");
        }
    }

    public void Disconnect()
    {
        colyseusClient.Disconnect();
        
        // Destroy player game objects
        foreach (KeyValuePair<string, GameObject> entry in remotePlayers)
        {
            Destroy(entry.Value);
        }
        remotePlayers.Clear();
        CCLogger.Log(CCLogger.EVENT_DISCONNECT, "disconnected");
        refreshUI();
    }

    void updateLocalAvatar()
    {
        if (localPlayerAvatar == null)
        {
            localPlayerAvatar = GameObject.FindWithTag("LocalPlayerAvatar");
        }
        Color playerColor = manager.LocalPlayerColor;
        if (localPlayerAvatar)
        {
            localPlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            localPlayerAvatar.name = "localPlayer_" + manager.LocalUsername;
        }
    }

    void updatePlayerList()
    {
        // networkUI.DebugMessage = colyseusClient.GetClientList();
        string listOfPlayersForDebug = "";
        foreach (var p in remotePlayers.Keys)
        {
            listOfPlayersForDebug = listOfPlayersForDebug + p + " \n";
        }
        if (!string.IsNullOrEmpty(listOfPlayersForDebug)) Debug.Log(listOfPlayersForDebug);

        networkUI.DebugMessage = listOfPlayersForDebug;
        networkUI.Username = manager.LocalUsername;
    }

    public void OnPlayerAdd(Player player)
    {
        bool isLocal = manager.LocalUsername == player.username;
        Debug.Log("Player add! playerName: " + player.username + " playerId: " + player.id + "is local: " + isLocal);
        Vector3 pos = Utils.NetworkV3ToVector3(player.playerPosition.position);
        Quaternion rot = Utils.NetworkV3ToQuaternion(player.playerPosition.rotation);
        if (isLocal)
        {
            localPlayer = player;
            AnnotationTool annotationTool = FindObjectOfType<AnnotationTool>();
            if (annotationTool)
            {
                annotationTool.SyncMyAnnotations();
            }
            updateLocalAvatar();
        }
        else
        {
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, rot);
            Color playerColor = SimulationManager.GetInstance().GetColorForUsername(player.username);
            remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            remotePlayerAvatar.name = "remotePlayer_" + player.username;
            remotePlayerAvatar.AddComponent<RemotePlayerMovement>();

            // add "player" to map of players
            if (!remotePlayers.ContainsKey(player.username))
            {
                remotePlayers.Add(player.username, remotePlayerAvatar);
            }
            else
            {
                remotePlayers[player.username] = remotePlayerAvatar;
            }
            
            // sync their annotations
            for (int i = 0; i < player.annotations.Count; i++)
            {
                NetworkTransform annotation = player.annotations[i];
                SimulationEvents.GetInstance().AnnotationReceived.Invoke(annotation, player);
            }
        }
        updatePlayerList();
    }

    public void OnPlayerRemove(Player player)
    {
        SimulationEvents.GetInstance().AnnotationClear.Invoke(player.username);
        GameObject remotePlayerAvatar;
        remotePlayers.TryGetValue(player.username, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);
        remotePlayers.Remove(player.username);
        updatePlayerList();
    }
                    
    public void OnPlayerChange(Player updatedPlayer)
    {
        // All player updates pass through here, including movement and interactions
        // though we need to handle those interactions differently. Keep this purely for movement!
        bool isLocal = updatedPlayer.username == localPlayer.username;
        
        Debug.Log("player id " + updatedPlayer.id + " is local: " + isLocal);

        GameObject remotePlayerAvatar;
        remotePlayers.TryGetValue(updatedPlayer.username, out remotePlayerAvatar);
        
        if (!isLocal)
        {
            if (remotePlayerAvatar != null)
            {
                // only move players with an avatar
                remotePlayerAvatar.GetComponent<RemotePlayerMovement>().NextPosition = new Vector3(
                    updatedPlayer.playerPosition.position.x, updatedPlayer.playerPosition.position.y,
                    updatedPlayer.playerPosition.position.z);
                remotePlayerAvatar.GetComponent<RemotePlayerMovement>().NextRotation = Quaternion.Euler(new Vector3(
                    updatedPlayer.playerPosition.rotation.x, updatedPlayer.playerPosition.rotation.y,
                    updatedPlayer.playerPosition.rotation.z));
            }
        }
    }
    private bool isLocalPlayer(Player player)
    {
        return player.username == localPlayer.username; 
    }

    private bool isRemotePlayer(Player player)
    {
        return !isLocalPlayer(player);
    }

    public void HandleNetworkInteraction(Player player, string interactionType)
    { 
        if (isRemotePlayer(player))
        {
            InteractionController interactionController = FindObjectOfType<InteractionController>();
            interactionController.HandleRemoteInteraction(player, interactionType);
        }
        else
        {
            Debug.Log("Ignoring local interaction from network ... ");
        }
    }

    public void HandleAnnotationDelete(Player player, string annotationName)
    {
        GameObject deletedAnnotation = GameObject.Find(annotationName);
        if (deletedAnnotation != null)
        {
            Debug.Log("received AnnotationDelete for " + annotationName);
            Destroy(deletedAnnotation);
        }
        else
        {
            Debug.Log("Could not delete " + annotationName + " for player " + player.username);
        }
        
    }
    public void BroadcastEarthInteraction(Vector3 pos, Quaternion rot)
    {
        colyseusClient.SendNetworkTransformUpdate(pos, rot, Vector3.one, "", "interaction");
    }

    public void BroadcastCelestialInteraction(NetworkCelestialObject celestialObj)
    {
        colyseusClient.SendCelestialInteraction(celestialObj);
    }

    public void BroadcastPlayerMovement(Vector3 pos, Quaternion rot)
    {
        colyseusClient.SendNetworkTransformUpdate(pos, rot, Vector3.one, "","movement");
    }

    public void BroadcastAnnotation(Vector3 pos, Quaternion rot, Vector3 scale, string annotationName)
    {
        Debug.Log("Broadcasting new Annotation event " + pos);
        colyseusClient.SendNetworkTransformUpdate(pos, rot, scale, annotationName, "annotation");
    }
    public void BroadcastDeleteAnnotation(string annotationName)
    {
        Debug.Log("Broadcasting new Delete Annotation event " + annotationName);
        colyseusClient.SendAnnotationDelete(annotationName);
    }

    IEnumerator selfDestruct(GameObject indicatorObj)
    {
        yield return new WaitForSeconds(3.0f);
        if (indicatorObj)
        {
            Destroy(indicatorObj);
        }
    }
    // called when the game is terminated
    void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SimulationEvents.GetInstance().AnnotationAdded.RemoveListener(BroadcastAnnotation);
        SimulationEvents.GetInstance().AnnotationDeleted.RemoveListener(BroadcastDeleteAnnotation);
    }
    
}