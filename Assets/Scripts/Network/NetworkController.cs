using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;
using static SimulationConstants;

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
        CCDebug.Log("OnSceneLoaded: " + scene.name);
        FindDependencies();
        if (IsConnected && scenesWithAvatars.Contains(scene.name))
        {
            // show the avatar
            updateLocalAvatar();
            
            foreach (string p in remotePlayerAvatars.Keys)
            {
                NetworkPlayer remoteNetworkPlayer = colyseusClient.GetPlayerById(p);
                updateAvatarForPlayer(remoteNetworkPlayer);
            }
        }
       
        refreshUI();
        CCLogger.Log(LOG_EVENT_SCENE, "OnSceneLoaded: " + scene.name);
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
            CCLogger.Log(LOG_EVENT_CONNECT, "connected");
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
            CCDebug.Log("Already disconnected");
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
        CCLogger.Log(LOG_EVENT_DISCONNECT, "disconnected");
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
            localPlayerAvatar.name = NETWORK_LOCAL_PLAYER_PREFIX + manager.LocalUsername;
        }
    }

    void updatePlayerList()
    {
        string listOfPlayersForDebug = "";
        foreach (var p in remotePlayerAvatars.Keys)
        {
            listOfPlayersForDebug = listOfPlayersForDebug + p + " \n";
        }
        if (!string.IsNullOrEmpty(listOfPlayersForDebug)) CCDebug.Log(listOfPlayersForDebug, LogLevel.Info, LogMessageCategory.Networking);

        networkUI.DebugMessage = listOfPlayersForDebug;
        networkUI.Username = manager.LocalUsername;
    }

    public void OnPlayerAdd(NetworkPlayer networkPlayer)
    {
        bool isLocal = manager.LocalUsername == networkPlayer.username;
        CCDebug.Log("Player add! playerName: " + networkPlayer.username + " playerId: " + networkPlayer.id + "is local: " + isLocal, LogLevel.Info, LogMessageCategory.Networking);
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
            BroadcastPinUpdated(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
            
            // Because avatars are the only game objects controlled in this class, this is where we update the avatar.
            updateLocalAvatar();
        }
        else
        {
            // Set up the remote player with the main manager
            manager.AddOrUpdateRemotePlayer(networkPlayer.username);
            
            // update their avatar
            updateAvatarForPlayer(networkPlayer);

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
                    CCDebug.Log("Player joined with locationPin time: " + networkPlayer.locationPin.datetime);
                }

                manager.GetRemotePlayer(networkPlayer.username).Pin = pin;
                
                interactionController.AddOrUpdatePin(pin, UserRecord.GetColorForUsername(networkPlayer.username), networkPlayer.username, 
                    false);
            }
            SimulationEvents.GetInstance().PlayerJoined.Invoke(networkPlayer.username);
        }
        updatePlayerList();
    }

    void updateAvatarForPlayer(NetworkPlayer networkPlayer)
    {
        Vector3 pos = Utils.NetworkV3ToVector3(networkPlayer.playerPosition.position);
        Quaternion rot = Utils.NetworkV3ToQuaternion(networkPlayer.playerPosition.rotation);
        string avatarName =  SimulationConstants.NETWORK_REMOTE_PLAYER_PREFIX + networkPlayer.username;
        GameObject playerAvatar = GameObject.Find(avatarName);
        if (playerAvatar == null)
        {
            playerAvatar = Instantiate(avatar, pos, rot);
            Color playerColor = UserRecord.GetColorForUsername(networkPlayer.username);
            playerAvatar.GetComponent<Renderer>().material.color = playerColor;
            playerAvatar.name = avatarName;
            playerAvatar.AddComponent<RemotePlayerMovement>();

            // add player avatar to map of player avatars
            remotePlayerAvatars.Add(networkPlayer.username, playerAvatar);
        }
        else
        {
            if (remotePlayerAvatars.ContainsKey(networkPlayer.username))
            {
                // move existing avatar
                playerAvatar = remotePlayerAvatars[networkPlayer.username];
                playerAvatar.transform.position = pos;
                playerAvatar.transform.rotation = rot;
            }
            else
            {
                remotePlayerAvatars[networkPlayer.username] = playerAvatar;
            }
        }
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
        
        CCDebug.Log("player id " + updatedNetworkPlayer.id + " is local: " + isLocal, LogLevel.Verbose, LogMessageCategory.Networking);

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
            CCDebug.Log("Ignoring local interaction from network ... ");
        }
    }

    /// <summary>
    /// Handle the deletion of an annotation.
    /// This message has a slightly different format than normal interactions, since it contains metadata
    /// which is the name of the deleted annotation. From this name, we can determine the user who deleted the annotation.
    /// </summary>
    /// <param name="networkPlayer">The network player who deleted an annotation</param>
    /// <param name="annotationName">The name of the annotation game object</param>
    public void HandleAnnotationDelete(NetworkPlayer networkPlayer, string annotationName)
    {
        CCDebug.Log($"Delete annotation event received for player {networkPlayer} {annotationName}", LogLevel.Info, LogMessageCategory.Interaction);
        GameObject deletedAnnotation = GameObject.Find(annotationName);
        if (deletedAnnotation != null)
        {
            CCDebug.Log("received AnnotationDelete for " + annotationName, LogLevel.Verbose, LogMessageCategory.Interaction);
            Destroy(deletedAnnotation);
        }
        else
        {
            CCDebug.Log("Could not delete " + annotationName + " for player " + networkPlayer.username, LogLevel.Info, LogMessageCategory.Interaction);
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
        CCDebug.Log("Broadcasting new Annotation event " + pos, LogLevel.Verbose, LogMessageCategory.Networking);
        colyseusClient.SendNetworkTransformUpdate(pos, rot, scale, annotationName, "annotation");
    }
    public void BroadcastDeleteAnnotation(string annotationName)
    {
        CCDebug.Log("Broadcasting new Delete Annotation event " + annotationName, LogLevel.Verbose, LogMessageCategory.Networking);
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
