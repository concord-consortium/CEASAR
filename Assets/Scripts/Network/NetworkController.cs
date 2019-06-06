using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public Button connectButton, leaveButton, clientListButton;
    public InputField m_EndpointField;
    public Text connectionStatusText;
    private string _connectionStatusMessage;

    public GameObject avatar;
    private Player localPlayer;
    private GameObject localPlayerAvatar;
    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected ColyseusClient colyseusClient;

    protected IndexedDictionary<Player, GameObject> players = new IndexedDictionary<Player, GameObject>();

    public TMPro.TMP_Text debugMessages;

    private string localPlayerName = "";

    private float lastUpdate;

    public bool autoConnect = false;
    public List<string> scenesWithAvatars;

    public string ServerStatusMessage
    {
        set
        {
            _connectionStatusMessage = value;
            if (connectionStatusText) connectionStatusText.text = value;
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
        leaveButton.onClick.AddListener(Disconnect);

        if (autoConnect)
        {
            ConnectToServer();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        if (scenesWithAvatars.Contains(scene.name))
        {
            OnPlayerAdd(localPlayer, true);
            foreach (Player p in players.Keys)
            {
                OnPlayerAdd(p, false);
            }
        }
    }

    void ConnectToServer()
    {
        /*
         * Get Colyseus endpoint from InputField
         *  for localhost use ws://localhost:2567/        
         */
        if (!colyseusClient.IsConnected)
        {
            if (string.IsNullOrEmpty(localPlayerName))
            {
                SimulationManager manager = SimulationManager.GetInstance();
                localPlayerName = manager.GenerateUsername();
                Debug.Log(localPlayerName);
            }
            string _localEndpoint = "ws://localhost:2567/";
            string _remoteEndpoint = "wss://calm-meadow-14344.herokuapp.com";
#if UNITY_EDITOR
            string endpoint = _remoteEndpoint; //_localEndpoint;
#else
            string endpoint = _remoteEndpoint; 
#endif
            // allow user to specify an endpoint
            endpoint = string.IsNullOrEmpty(m_EndpointField.text) ? endpoint : m_EndpointField.text;
            colyseusClient.ConnectToServer(endpoint, localPlayerName);
        }
    }
    void Disconnect()
    {
        colyseusClient.Disconnect();

        // Destroy player game objects
        foreach (KeyValuePair<Player, GameObject> entry in players)
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
        Debug.Log("Player add! x => " + player.x + ", y => " + player.y + " playerName:" + player.username);

        Vector3 pos = new Vector3(player.x, player.y, 0);
        if (isLocal)
        {
            localPlayer = player;
            localPlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            Color playerColor = SimulationManager.GetInstance().GetColorForUsername(player.username);
            localPlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            localPlayerAvatar.name = "localPlayer_" + player.username;
        }
        else
        {
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            Color playerColor = SimulationManager.GetInstance().GetColorForUsername(player.username);
            remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            remotePlayerAvatar.name = "remotePlayer_" + player.username;
            // add "player" to map of players
            if (!players.ContainsKey(player))
            {
                players.Add(player, remotePlayerAvatar);
            }
            else
            {
                players[player] = remotePlayerAvatar;
            }
        }
        debugMessages.text = colyseusClient.GetClientList();
    }

    public void OnPlayerRemove(Player player)
    {
        GameObject remotePlayerAvatar;
        players.TryGetValue(player, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);

        players.Remove(player);
        debugMessages.text = colyseusClient.GetClientList();
    }


    public void OnPlayerMove(Player player)
    {
        GameObject remotePlayerAvatar;
        players.TryGetValue(player, out remotePlayerAvatar);

        Debug.Log(player.x);
        if (remotePlayerAvatar)
        {
            remotePlayerAvatar.transform.Translate(new Vector3(player.x, player.y, 0));
        }
        else
        {
            localPlayerAvatar.transform.Translate(new Vector3(player.x, player.y, 0));
        }
    }

    // called when the game is terminated
    void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}