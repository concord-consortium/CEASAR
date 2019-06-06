using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    public System.Random rng = new System.Random();
    // UI Buttons are attached through Unity Inspector
    public Button connectButton, leaveButton, clientListButton;
    public InputField m_EndpointField;
    public Text connectionStatusText;

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

    private void choosePlayerName()
    {
        TextAsset colorList = Resources.Load("colors") as TextAsset;
        TextAsset animalList = Resources.Load("animals") as TextAsset;
        string[] colors = colorList.text.Split('\n');
        string[] animals = animalList.text.Split('\n');

        localPlayerName = colors[rng.Next(colors.Length - 1)] + animals[rng.Next(animals.Length - 1)] + rng.Next(999);
        Debug.Log(localPlayerName);

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
                choosePlayerName();
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
        connectionStatusText.text = "Disconnected";

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
            localPlayerAvatar.name = "localPlayer_" + player.username;
            connectionStatusText.text = "Connected as " + player.username;
        }
        else
        {
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            Color playerColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
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
}