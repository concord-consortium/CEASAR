using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System;
using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

public class ColyseusClient : MonoBehaviour
{

    // UI Buttons are attached through Unity Inspector
    public Button m_ConnectButton, m_JoinButton, m_ReJoinButton, m_SendMessageButton, m_LeaveButton, m_GetAvailableRoomsButton;
    public InputField m_EndpointField;
    public Text m_IdText, m_SessionIdText;

    public GameObject userAvatar;
    private GameObject playerAvatar;
    // here's where we can set up 4-digit pins for a collab room
    public string roomName = "demo";

    protected Client client;
    protected Room<State> room;

    protected IndexedDictionary<Player, GameObject> players = new IndexedDictionary<Player, GameObject>();

    public TMPro.TMP_Text debugMessages;

    // Use this for initialization
    IEnumerator Start()
    {
        /* Demo UI */
        m_ConnectButton.onClick.AddListener(ConnectToServer);

        m_JoinButton.onClick.AddListener(JoinRoom);
        m_ReJoinButton.onClick.AddListener(ReJoinRoom);
        m_SendMessageButton.onClick.AddListener(SendNetworkMessage);
        m_LeaveButton.onClick.AddListener(LeaveRoom);
        m_GetAvailableRoomsButton.onClick.AddListener(GetAvailableRooms);

        /* Always call Recv if Colyseus connection is open */
        while (true)
        {
            if (client != null)
            {
                client.Recv();
            }
            yield return 0;
        }
    }

    async void ConnectToServer()
    {
        /*
         * Get Colyseus endpoint from InputField
         */
        string endpoint = string.IsNullOrEmpty(m_EndpointField.text) ? "ws://localhost:2567" : m_EndpointField.text;

        Debug.Log("Connecting to " + endpoint);

        /*
         * Connect into Colyeus Server
         */
        client = new Client(endpoint);

        await client.Auth.Login();
        var friends = await client.Auth.GetFriends();

        // Update username
        client.Auth.Username = "MyUsername";
        await client.Auth.Save();

        client.OnOpen += (object sender, EventArgs e) =>
        {
            /* Update Demo UI */
            m_IdText.text = "id: " + client.Id;
            Debug.Log("joining room");
            JoinRoom();
        };
        client.OnError += (sender, e) => Debug.LogError(e.Message);
        client.OnClose += (sender, e) => Debug.Log("CONNECTION CLOSED");
        StartCoroutine(client.Connect());
    }

    void JoinRoom()
    {
        bool canJoinExisting = false;
        string availableRoomID = "";
        client.GetAvailableRooms(roomName, (RoomAvailable[] roomsAvailable) =>
        {
            Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
            canJoinExisting = roomsAvailable.Length > 0;
            for (var i = 0; i < roomsAvailable.Length; i++)
            {
                Debug.Log("roomId: " + roomsAvailable[i].roomId);
                Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
                Debug.Log("clients: " + roomsAvailable[i].clients);
                Debug.Log("metadata: " + roomsAvailable[i].metadata);

                if (canJoinExisting && i == 0)
                {
                    availableRoomID = roomsAvailable[i].roomId;
                }
            }


        });
        string roomToJoin = canJoinExisting ? availableRoomID : roomName;
        room = client.Join<State>(roomToJoin, new Dictionary<string, object>()
        {
            { "create", true }
        });

        room.OnReadyToConnect += (sender, e) =>
        {
            Debug.Log("Ready to connect to room!");
            StartCoroutine(room.Connect());
        };
        room.OnError += (sender, e) =>
        {
            Debug.LogError(e.Message);
        };
        room.OnJoin += (sender, e) =>
        {
            Debug.Log("Joined room successfully.");
            m_SessionIdText.text = "sessionId: " + room.SessionId;
            playerAvatar = Instantiate(userAvatar);
            playerAvatar.name = "localPlayer_";

            room.State.players.OnAdd += OnPlayerAdd;
            room.State.players.OnRemove += OnPlayerRemove;
            room.State.players.OnChange += OnPlayerMove;

            PlayerPrefs.SetString("sessionId", room.SessionId);
            PlayerPrefs.Save();
        };

        room.OnStateChange += OnStateChangeHandler;
        room.OnMessage += OnMessage;
    }

    void ReJoinRoom()
    {
        string sessionId = PlayerPrefs.GetString("sessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.Log("Cannot ReJoin without having a sessionId");
            return;
        }

        room = client.ReJoin<State>(roomName, sessionId);

        room.OnReadyToConnect += (sender, e) =>
        {
            Debug.Log("Ready to connect to room!");
            StartCoroutine(room.Connect());
        };
        room.OnError += (sender, e) => Debug.LogError(e.Message);
        room.OnJoin += (sender, e) =>
        {
            Debug.Log("Joined room successfully.");
            m_SessionIdText.text = "sessionId: " + room.SessionId;

            room.State.players.OnAdd += OnPlayerAdd;
            room.State.players.OnRemove += OnPlayerRemove;
            room.State.players.OnChange += OnPlayerMove;
        };

        room.OnStateChange += OnStateChangeHandler;
        room.OnMessage += OnMessage;
    }

    void LeaveRoom()
    {
        Debug.Log("closing connection");
        room.Leave(false);

        // Destroy player players
        foreach (KeyValuePair<Player, GameObject> entry in players)
        {
            Destroy(entry.Value);
        }

        players.Clear();
        // closing client connection
        m_IdText.text = "disconnected";
        client.Close();
        Destroy(playerAvatar);
    }

    void GetAvailableRooms()
    {
        client.GetAvailableRooms(roomName, (RoomAvailable[] roomsAvailable) =>
        {
            Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
            for (var i = 0; i < roomsAvailable.Length; i++)
            {
                Debug.Log("roomId: " + roomsAvailable[i].roomId);
                Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
                Debug.Log("clients: " + roomsAvailable[i].clients);
                Debug.Log("metadata: " + roomsAvailable[i].metadata);
            }
        });
    }

    void SendNetworkMessage()
    {
        if (room != null)
        {
            room.Send("message");
        }
        else
        {
            Debug.Log("Room is not connected!");
        }
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Message);
        //var message = (IndexedDictionary<string, object>)e.Message;

    }

    void OnStateChangeHandler(object sender, StateChangeEventArgs<State> e)
    {
        // Setup room first state
        Debug.Log("State has been updated!");
        Debug.Log(e.State);
    }

    void OnPlayerAdd(object sender, KeyValueEventArgs<Player, string> item)
    {

        Debug.Log("Player add! x => " + item.Value.x + ", y => " + item.Value.y);

        Vector3 pos = new Vector3(item.Value.x, item.Value.y, 0);
        GameObject remotePlayerAvatar = Instantiate(userAvatar, pos, Quaternion.identity);
        Color playerColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
        remotePlayerAvatar.GetComponent<Renderer>().material.color = playerColor;
        remotePlayerAvatar.name = "remotePlayer_";
        // add "player" to map of players
        players.Add(item.Value, remotePlayerAvatar);
    }

    void OnPlayerRemove(object sender, KeyValueEventArgs<Player, string> item)
    {
        GameObject remotePlayerAvatar;
        players.TryGetValue(item.Value, out remotePlayerAvatar);
        Destroy(remotePlayerAvatar);

        players.Remove(item.Value);
    }


    void OnPlayerMove(object sender, KeyValueEventArgs<Player, string> item)
    {
        GameObject remotePlayerAvatar;
        players.TryGetValue(item.Value, out remotePlayerAvatar);

        Debug.Log(item.Value.x);

        remotePlayerAvatar.transform.Translate(new Vector3(item.Value.x, item.Value.y, 0));
    }

    void OnApplicationQuit()
    {
        // Make sure client will disconnect from the server
        if (room != null)
        {
            room.Leave();
        }

        if (client != null)
        {
            client.Close();
        }
    }

    void showLog(string text, bool clear)
    {
        if (debugMessages != null)
        {
            if (clear)
            {
                debugMessages.SetText(text);
            }
            else
            {
                debugMessages.SetText(debugMessages.text + "\n" + text);
            }
        }
    }
}