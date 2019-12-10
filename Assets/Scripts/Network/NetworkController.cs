using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;
using System.Text;

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public Button connectButton;
    public TMPro.TMP_Text connectButtonText;
    public InputField m_EndpointField;
    public Text connectionStatusText;
    private string _connectionStatusMessage;

    public GameObject avatar;
    private Player localPlayer;
    private GameObject localPlayerAvatar;
    public GameObject interactionIndicator;

    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected ColyseusClient colyseusClient;

    protected IndexedDictionary<string, GameObject> remotePlayers = new IndexedDictionary<string, GameObject>();

    public TMPro.TMP_Text debugMessages;

    private string localPlayerName = "";

    private float lastUpdate;

    public bool autoConnect = false;
    public List<string> scenesWithAvatars;

    public bool IsConnected
    {
        get { return colyseusClient != null && colyseusClient.IsConnected; }
    }

    public string ServerStatusMessage
    {
        set
        {
            _connectionStatusMessage = value;
            if (connectionStatusText) connectionStatusText.text = value;
            if (connectButtonText && IsConnected) connectButtonText.text = "Disconnect";
            if (connectButtonText && !IsConnected) connectButtonText.text = "Connect";
        }
    }
    public void NetworkPanelToggled(bool active)
    {
        if (active)
        {
            // refresh connection status
            ServerStatusMessage = _connectionStatusMessage;
            debugMessages.text = colyseusClient.GetClientList();
        }
    }

    public void SetNetworkAddress(string destination)
    {
        SimulationManager manager = SimulationManager.GetInstance();
        switch (destination)
        {
            case "local":
                m_EndpointField.text = manager.LocalNetworkServer;
                break;
            case "dev":
                m_EndpointField.text = manager.DevNetworkServer;
                break;
            case "web":
                m_EndpointField.text = manager.ProductionNetworkServer;
                break;
            default:
                m_EndpointField.text = manager.ProductionNetworkServer;
                break;
        }
    }

    // Need this so the network UI persists across scenes
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        colyseusClient = GetComponent<ColyseusClient>();
        /* Demo UI */
        connectButton.onClick.AddListener(ConnectToServer);

        if (autoConnect)
        {
            ConnectToServer();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        if (IsConnected && scenesWithAvatars.Contains(scene.name))
        {
            OnPlayerAdd(localPlayer, true);
            foreach (string p in remotePlayers.Keys)
            {
                Player remotePlayer = colyseusClient.GetPlayerById(p);
                OnPlayerAdd(remotePlayer, false);
            }
        }
    }

    void ConnectToServer()
    {
        /*
         * Get Colyseus endpoint from InputField
         *  for localhost use ws://localhost:2567/        
         */
        if (!IsConnected)
        {
            SimulationManager manager = SimulationManager.GetInstance();
            if (string.IsNullOrEmpty(localPlayerName))
            {
                localPlayerName = manager.GenerateUsername();
                manager.LocalPlayerColor = manager.GetColorForUsername(localPlayerName);
                Debug.Log(localPlayerName);
            }
            string _localEndpoint = manager.LocalNetworkServer;
            string _remoteEndpoint = manager.ProductionNetworkServer;
#if UNITY_EDITOR
            string endpoint = _localEndpoint;
#else
            string endpoint = _remoteEndpoint; 
#endif
            // allow user to specify an endpoint
            endpoint = string.IsNullOrEmpty(m_EndpointField.text) ? endpoint : m_EndpointField.text;
            colyseusClient.ConnectToServer(endpoint, localPlayerName);
        }
        else if (IsConnected) Disconnect();
        else
        {
            Debug.Log("Already disconnected");
        }
    }
    void Disconnect()
    {
        colyseusClient.Disconnect();

        // Destroy player game objects
        foreach (KeyValuePair<string, GameObject> entry in remotePlayers)
        {
            Destroy(entry.Value);
        }

        // closing client connection

        if (localPlayerAvatar) Destroy(localPlayerAvatar);
        debugMessages.text = "";
    }

    void SendNetworkMessage()
    {
        colyseusClient.SendNetworkMessage("message");
    }

    void OnMessage(string message)
    {
        Debug.Log(message);
    }

    public void OnPlayerAdd(Player player, bool isLocal)
    {
        Debug.Log("Player add! x => " + player.x + ", y => " + player.y + " playerName:" + player.username + " playerId: " + player.id);

        Vector3 pos = Utils.NetworkPosToPosition(player.playerPosition.position);
        Quaternion rot = Utils.NetworkRotToRotation(player.playerPosition.rotation);
        if (isLocal)
        {
            localPlayer = player;
            Transform avatarCamera = GameObject.FindWithTag("Player").transform;
            localPlayerAvatar = avatarCamera.Find("Avatar").gameObject;
            Color playerColor = SimulationManager.GetInstance().LocalPlayerColor;
            localPlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            localPlayerAvatar.name = "localPlayer_" + player.username;
        }
        else
        {
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, rot);
            Color playerColor = SimulationManager.GetInstance().GetColorForUsername(player.username);
            remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            remotePlayerAvatar.name = "remotePlayer_" + player.username;
            remotePlayerAvatar.AddComponent<RemotePlayerMovement>();
            // add "player" to map of players
            if (!remotePlayers.ContainsKey(player.id))
            {
                remotePlayers.Add(player.id, remotePlayerAvatar);
            }
            else
            {
                remotePlayers[player.id] = remotePlayerAvatar;
            }
        }
        debugMessages.text = colyseusClient.GetClientList();
        string listOfPlayersForDebug = "";
        foreach (var p in remotePlayers.Keys)
        {
            listOfPlayersForDebug = listOfPlayersForDebug + p + " \n";
        }
        Debug.Log(listOfPlayersForDebug);
    }

    public void OnPlayerRemove(Player player)
    {
        GameObject remotePlayerAvatar;
        remotePlayers.TryGetValue(player.id, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);

        remotePlayers.Remove(player.id);
        debugMessages.text = colyseusClient.GetClientList();
    }


    public void OnPlayerChange(Player updatedPlayer)
    {
        bool isLocal = updatedPlayer.id == localPlayer.id;
        bool isKnownRemotePlayer = remotePlayers.Keys.Contains(updatedPlayer.id);   // ToList().First(p => p.id == playerId);
        string knownPlayerName = isKnownRemotePlayer ? updatedPlayer.username : "unknown";

        Debug.Log("player id " + updatedPlayer.id + " is local: " + isLocal + " isKnown: " + knownPlayerName);

        GameObject remotePlayerAvatar;
        remotePlayers.TryGetValue(updatedPlayer.id, out remotePlayerAvatar);
        if (isLocal)
        {
            Debug.Log("self update");
        }
        else
        {
            if (updatedPlayer.interactionTarget != null)
            {
                Debug.Log("Interaction update: " + updatedPlayer.interactionTarget.position.x + "," + updatedPlayer.interactionTarget.position.y + "," + updatedPlayer.interactionTarget.position.z);
                Vector3 pos = Utils.NetworkPosToPosition(updatedPlayer.interactionTarget.position);
                Quaternion rot = Utils.NetworkRotToRotation(updatedPlayer.interactionTarget.rotation);
                ShowInteraction(pos, rot, SimulationManager.GetInstance().GetColorForUsername(updatedPlayer.username), false);
            }
            Debug.Log("Movement update: " + updatedPlayer.playerPosition);
            if (remotePlayerAvatar != null)
            {
                remotePlayerAvatar.GetComponent<RemotePlayerMovement>().NextPosition = new Vector3(updatedPlayer.playerPosition.position.x, updatedPlayer.playerPosition.position.y, updatedPlayer.playerPosition.position.z);
                // remotePlayerAvatar.transform.position = new Vector3(updatedPlayer.playerPosition.position.x, updatedPlayer.playerPosition.position.y, updatedPlayer.playerPosition.position.z);
            }
            else
            {
                Debug.Log("Ghost player with no avatar!");
            }
        }

    }

    public void ShowInteraction(Vector3 pos, Quaternion rot, Color playerColor, bool isLocal)
    {
        if (interactionIndicator)
        {
            GameObject indicatorObj = Instantiate(interactionIndicator);
            indicatorObj.transform.localRotation = rot;
            indicatorObj.transform.position = pos;
            Utils.SetObjectColor(indicatorObj, playerColor);
            StartCoroutine(selfDestruct(indicatorObj));
        }
        if (isLocal)
        {
            // now need to broadcast to remotes
            colyseusClient.SendInteraction(pos, rot, playerColor);

        }

    }

    public void HandleMovementUpdate(Vector3 pos, Quaternion rot, bool isLocal)
    {
        if (isLocal)
        {
            // broadcast!
            colyseusClient.SendMovement(pos, rot);
        }
        else
        {
            // move remote player
        }
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
    }
}