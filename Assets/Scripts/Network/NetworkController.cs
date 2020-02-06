using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    public GameObject avatar;
    private NetworkPlayer _localNetworkPlayer;
    private GameObject localPlayerAvatar;
    public GameObject interactionIndicator;

    protected ColyseusClient colyseusClient;

    protected IndexedDictionary<string, GameObject> remotePlayerAvatars = new IndexedDictionary<string, GameObject>();

    public bool autoConnect = false;
    public List<string> scenesWithAvatars;

    public ServerRecord networkConnection = ServerList.Web;

    public NetworkUI networkUI;
    private ServerRecord _selectedNetwork = ServerList.Web;

    SimulationManager manager;

    private bool _isConnected = false;
    public bool IsConnected
    {
        get { return colyseusClient != null && colyseusClient.IsConnected; }
    }
    public bool IsConnecting
    {
        get { return colyseusClient != null && colyseusClient.IsConnecting; }
    }

    public string ServerStatusMessage {
        set { networkUI.ConnectionStatusText = value; }
    }

    public bool useDev = false;
 
    // Need this so the network UI persists across scenes
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        colyseusClient = GetComponent<ColyseusClient>();
    }

    public void Setup()
    {
        FindDependencies();
        networkUI.Username = manager.LocalUsername;
        if (autoConnect)
        {
            ConnectToServer();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        SimulationEvents.GetInstance().AnnotationAdded.AddListener(BroadcastAnnotation);
        SimulationEvents.GetInstance().AnnotationDeleted.AddListener(BroadcastDeleteAnnotation);
        SimulationEvents.GetInstance().PushPinUpdated.AddListener(BroadcastPinUpdated);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        FindDependencies();
        if (IsConnected && scenesWithAvatars.Contains(scene.name))
        {
            OnPlayerAdd(_localNetworkPlayer);
            foreach (string p in remotePlayerAvatars.Keys)
            {
                NetworkPlayer remoteNetworkPlayer = colyseusClient.GetPlayerById(p);
                OnPlayerAdd(remoteNetworkPlayer);
            }
        }
       
        refreshUI();
        CCLogger.Log(CCLogger.EVENT_SCENE, "OnSceneLoaded: " + scene.name);
    }

    // TODO: Decouple depedency tree using events
    // For now we just need to ensure our peers exist OnSceneLoaded ...
    private void FindDependencies()
    {
        manager = SimulationManager.GetInstance();
        colyseusClient = GetComponent<ColyseusClient>();
        networkUI = FindObjectOfType<NetworkUI>();
        colyseusClient = GetComponent<ColyseusClient>();
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
        if(networkUI)
        {
            networkUI.ConnectedStatusUpdate(IsConnected);
            updatePlayerList();
        }
        
    }


    public void SetNetworkAddress(ServerRecord destination)
    {
        _selectedNetwork = destination;
    }

    // Use this if you have a valid enpoint in mind:
    public void ConnectToEndpoint(string endpoint)
    {
        if (!IsConnected)
        {
            UserRecord user = SimulationManager.GetInstance().LocalPlayerRecord; 
            FindDependencies();
            colyseusClient.ConnectToServer(endpoint, user.Username, user.group);
            manager.server = ServerList.Custom;
            manager.server.address = endpoint;
            refreshUI();
            CCLogger.Log(CCLogger.EVENT_CONNECT, "connected");
        }
    }

    // Use this if you want to connect with reasonable defaults
    public void ConnectToServer(string userDefinedEndpoint = null)
    {
        /*
         * Get Colyseus endpoint from InputField
         *  for localhost use ws://localhost:2567/        
         */
        if (!IsConnected)
        {

            string endpoint = string.Empty;

            if (string.IsNullOrEmpty(userDefinedEndpoint))
            {
                // no user interaction with the network address, work on some defaults
#if UNITY_EDITOR
                endpoint = ServerList.Local.address;
#else
                
                endpoint = ServerList.Web.address;
#endif
            }
            else
            {
                endpoint = userDefinedEndpoint;
            }

            if (useDev)
            {
                endpoint = ServerList.Dev.address;
            }
            ConnectToEndpoint(endpoint);
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
        foreach (KeyValuePair<string, GameObject> entry in remotePlayerAvatars)
        {
            Destroy(entry.Value);
        }
        remotePlayerAvatars.Clear();
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
        string listOfPlayersForDebug = "";
        foreach (var p in remotePlayerAvatars.Keys)
        {
            listOfPlayersForDebug = listOfPlayersForDebug + p + " \n";
        }
        if (!string.IsNullOrEmpty(listOfPlayersForDebug)) Debug.Log(listOfPlayersForDebug);

        networkUI.DebugMessage = listOfPlayersForDebug;
        networkUI.Username = manager.LocalUsername;
    }

    public void OnPlayerAdd(NetworkPlayer networkPlayer)
    {
        bool isLocal = manager.LocalUsername == networkPlayer.username;
        Debug.Log("Player add! playerName: " + networkPlayer.username + " playerId: " + networkPlayer.id + "is local: " + isLocal);
        Vector3 pos = Utils.NetworkV3ToVector3(networkPlayer.playerPosition.position);
        Quaternion rot = Utils.NetworkV3ToQuaternion(networkPlayer.playerPosition.rotation);

        InteractionController interactionController = FindObjectOfType<InteractionController>();
        if (isLocal)
        {
            _localNetworkPlayer = networkPlayer;
            AnnotationTool annotationTool = FindObjectOfType<AnnotationTool>();
            if (annotationTool)
            {
                annotationTool.SyncMyAnnotations();
            }

            // update the server with current perspective pin for local user
            BroadcastPinUpdated(manager.LocalUserPin, manager.LocalPlayerLookDirection);
            
            // Because avatars are the only game objects controlled in this class, this is where we update the avatar.
            updateLocalAvatar();
        }
        else
        {
            // Set up the remote player with the main manager
            manager.AddRemotePlayer(networkPlayer.username);
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, rot);
            Color playerColor = UserRecord.GetColorForUsername(networkPlayer.username);
            remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            remotePlayerAvatar.name = "remotePlayer_" + networkPlayer.username;
            remotePlayerAvatar.AddComponent<RemotePlayerMovement>();
            
            // add "player" to map of player avatars
            if (!remotePlayerAvatars.ContainsKey(networkPlayer.username))
            {
                remotePlayerAvatars.Add(networkPlayer.username, remotePlayerAvatar);
            }
            else
            {
                remotePlayerAvatars[networkPlayer.username] = remotePlayerAvatar;
            }

            // sync their annotations
            for (int i = 0; i < networkPlayer.annotations.Count; i++)
            {
                NetworkTransform annotation = networkPlayer.annotations[i];
                SimulationEvents.GetInstance().AnnotationReceived.Invoke(annotation, networkPlayer);
            }
            if (interactionController && networkPlayer.locationPin != null)
            {
                Pushpin pin = interactionController.NetworkPlayerPinToPushpin(networkPlayer);

                if (networkPlayer.locationPin != null )
                {
                    Debug.Log("Player joined with locationPin time: " + networkPlayer.locationPin.datetime);
                }

                manager.GetRemotePlayer(networkPlayer.username).Pin = pin;
                
                interactionController.AddOrUpdatePin(pin, playerColor, networkPlayer.username, 
                    false);
            }
            SimulationEvents.GetInstance().PlayerJoined.Invoke(networkPlayer.username);
        }
        updatePlayerList();
    }

    public void OnPlayerRemove(NetworkPlayer networkPlayer)
    {
        SimulationEvents.GetInstance().PlayerLeft.Invoke(networkPlayer.username);
        SimulationEvents.GetInstance().AnnotationClear.Invoke(networkPlayer.username);
        // TODO: clear pushpins for player
        GameObject remotePlayerAvatar;
        remotePlayerAvatars.TryGetValue(networkPlayer.username, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);
        remotePlayerAvatars.Remove(networkPlayer.username);
        updatePlayerList();
    }
                    
    public void OnPlayerChange(NetworkPlayer updatedNetworkPlayer)
    {
        // All player updates pass through here, including movement and interactions
        // though we need to handle those interactions differently. Keep this purely for movement!
        bool isLocal = updatedNetworkPlayer.username == _localNetworkPlayer.username;
        
        Debug.Log("player id " + updatedNetworkPlayer.id + " is local: " + isLocal);

        GameObject remotePlayerAvatar;
        remotePlayerAvatars.TryGetValue(updatedNetworkPlayer.username, out remotePlayerAvatar);
        
        if (!isLocal)
        {
            if (remotePlayerAvatar != null)
            {
                // only move players with an avatar
                remotePlayerAvatar.GetComponent<RemotePlayerMovement>().NextPosition = new Vector3(
                    updatedNetworkPlayer.playerPosition.position.x, updatedNetworkPlayer.playerPosition.position.y,
                    updatedNetworkPlayer.playerPosition.position.z);
                remotePlayerAvatar.GetComponent<RemotePlayerMovement>().NextRotation = Quaternion.Euler(new Vector3(
                    updatedNetworkPlayer.playerPosition.rotation.x, updatedNetworkPlayer.playerPosition.rotation.y,
                    updatedNetworkPlayer.playerPosition.rotation.z));
            }
        }
    }
    private bool isLocalPlayer(NetworkPlayer networkPlayer)
    {
        return networkPlayer.username == _localNetworkPlayer.username; 
    }

    private bool isRemotePlayer(NetworkPlayer networkPlayer)
    {
        return !isLocalPlayer(networkPlayer);
    }

    public void HandleNetworkInteraction(NetworkPlayer networkPlayer, string interactionType)
    { 
        if (isRemotePlayer(networkPlayer))
        {
            InteractionController interactionController = FindObjectOfType<InteractionController>();
            interactionController.HandleRemoteInteraction(networkPlayer, interactionType);
        }
        else
        {
            Debug.Log("Ignoring local interaction from network ... ");
        }
    }

    public void HandleAnnotationDelete(NetworkPlayer networkPlayer, string annotationName)
    {
        GameObject deletedAnnotation = GameObject.Find(annotationName);
        if (deletedAnnotation != null)
        {
            Debug.Log("received AnnotationDelete for " + annotationName);
            Destroy(deletedAnnotation);
        }
        else
        {
            Debug.Log("Could not delete " + annotationName + " for player " + networkPlayer.username);
        }
        
    }
    public void BroadcastEarthInteraction(Vector3 pos, Quaternion rot)
    {
        colyseusClient.SendNetworkTransformUpdate(pos, rot, Vector3.one, "", "interaction");
    }
    public void BroadcastPinUpdated(Pushpin pin, Vector3 lookDirection)
    {
        colyseusClient.SendPinUpdate(pin.Location.Latitude, pin.Location.Longitude, pin.SelectedDateTime, lookDirection, pin.LocationName);
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
        SimulationEvents.GetInstance().PushPinUpdated.RemoveListener(BroadcastPinUpdated);
    }
    
    public NetworkPlayer GetNetworkPlayerByName(string name)
    {
        return colyseusClient.GetPlayerById(name);
    }
}
