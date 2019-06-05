using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using GameDevWare.Serialization;

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    public System.Random rng = new System.Random();
    // UI Buttons are attached through Unity Inspector
    public Button m_ConnectButton, m_SendMessageButton, m_LeaveButton, m_GetAvailableRoomsButton;
    public InputField m_EndpointField;
    public Text m_IdText, m_SessionIdText;

    public GameObject avatar;
    private GameObject localPlayerAvatar;
    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected ColyseusClient client;

    protected IndexedDictionary<Player, GameObject> players = new IndexedDictionary<Player, GameObject>();

    public TMPro.TMP_Text debugMessages;

    private string localPlayerName = "";

    private float lastUpdate;
    // Use this for initialization
    void Start()
    {

        client = GetComponent<ColyseusClient>();
        /* Demo UI */
        m_ConnectButton.onClick.AddListener(ConnectToServer);

        m_SendMessageButton.onClick.AddListener(SendNetworkMessage);
        m_LeaveButton.onClick.AddListener(Disconnect);

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
        if (client == null || string.IsNullOrEmpty(localPlayerName))
        {
            if (string.IsNullOrEmpty(localPlayerName))
                choosePlayerName();
            string _localEndpoint = "ws://localhost:2567/";
            string _remoteEndpoint = "ws://calm-meadow-14344.herokuapp.com";
#if UNITY_EDITOR
            string endpoint = _localEndpoint;
#else
            string endpoint = _remoteEndpoint; 
#endif
            // allow user to specify an endpoint
            endpoint = string.IsNullOrEmpty(m_EndpointField.text) ? endpoint : m_EndpointField.text;
            Debug.Log("Connecting to " + endpoint);
            client.ConnectToServer(endpoint, localPlayerName);
        }
    }
    void Disconnect()
    {
        client.Disconnect();

        // Destroy player game objects
        foreach (KeyValuePair<Player, GameObject> entry in players)
        {
            Destroy(entry.Value);
        }

        // closing client connection
        m_IdText.text = "disconnected";

        if (localPlayerAvatar) Destroy(localPlayerAvatar);
        localPlayerName = "";
        debugMessages.text = client.GetClientList();
    }

    void SendNetworkMessage()
    {
        client.SendNetworkMessage("message");
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
            localPlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            localPlayerAvatar.name = "localPlayer_" + player.username;
        }
        else
        {
            GameObject remotePlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            Color playerColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
            remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
            remotePlayerAvatar.name = "remotePlayer_" + player.username;
            // add "player" to map of players
            players.Add(player, remotePlayerAvatar);
        }
        debugMessages.text = client.GetClientList();
    }

    public void OnPlayerRemove(Player player)
    {
        GameObject remotePlayerAvatar;
        players.TryGetValue(player, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);

        players.Remove(player);
        debugMessages.text = client.GetClientList();
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