using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

public class ColyseusClient : MonoBehaviour
{
    public System.Random rng = new System.Random();

    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected Client client;
    protected Room<State> room;
    private NetworkController networkController;

    protected List<Player> players = new List<Player>();

    private string localPlayerName = "";

    private float lastUpdate;
    private bool connecting = false;
    public bool IsConnected
    {
        get { return client != null; }
    }
    // Use this for initialization
    IEnumerator Start()
    {
        networkController = GetComponent<NetworkController>();
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
    //private void Update()
    //{
    //    if (localPlayerAvatar != null && client.Id != null)
    //    {
    //        lastUpdate += Time.deltaTime;
    //        if (lastUpdate > 0.2)
    //        {
    //            // send update
    //            var pos = 0.01;
    //            if (localPlayerAvatar.transform.position.x > 5) pos *= -1;
    //            room.Send(new Dictionary<string, object>()
    //                {
    //                    {"x", pos }
    //                }
    //            );
    //            lastUpdate = 0;
    //        }
    //    }
    //}


    public void ConnectToServer(string serverEndpoint, string username)
    {
        if (!connecting && (!IsConnected || string.IsNullOrEmpty(localPlayerName)))
        {
            connecting = true;
            Debug.Log("Connecting to " + serverEndpoint);
            if (string.IsNullOrEmpty(localPlayerName)) localPlayerName = username;

            // Connect to Colyeus Server

            client = new Client(serverEndpoint);

            //await client.Auth.Login();
            //var friends = await client.Auth.GetFriends();

            //// Update username
            //client.Auth.Username = "MyUsername";
            //await client.Auth.Save();

            client.OnOpen += (object sender, EventArgs e) =>
            {
                Debug.Log("joining room");
                JoinRoom();
            };
            client.OnError += (sender, e) => Debug.LogError(e.Message);
            client.OnClose += (sender, e) => Debug.Log("CONNECTION CLOSED");
            StartCoroutine(client.Connect());
        }
    }
    public void Disconnect()
    {
        LeaveRoom();
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
            { "username", localPlayerName }
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
        localPlayerName = "";
        players.Clear();
        // closing client connection
        client.Close();
        connecting = false;
        client = null;
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
    public string GetClientList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Connected clients:");
        foreach (Player p in players)
        {
            sb.AppendLine(p.username);
        }
        return sb.ToString();
    }
    public void SendNetworkMessage(string message)
    {
        if (room != null)
        {
            room.Send(message);
        }
        else
        {
            Debug.Log("Room is not connected!");
        }
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log(e.Message);       //var message = (IndexedDictionary<string, object>)e.Message;

    }

    void OnStateChangeHandler(object sender, StateChangeEventArgs<State> e)
    {
        // Setup room first state
        Debug.Log("State has been updated!");
        Debug.Log(e.State);
    }

    void OnPlayerAdd(object sender, KeyValueEventArgs<Player, string> item)
    {
        if (!players.Contains(item.Value)) players.Add(item.Value);
        networkController.OnPlayerAdd(item.Value, item.Key == room.SessionId);
    }

    void OnPlayerRemove(object sender, KeyValueEventArgs<Player, string> item)
    {
        if (players.Contains(item.Value)) players.Remove(item.Value);
        networkController.OnPlayerRemove(item.Value);
    }

    void OnPlayerMove(object sender, KeyValueEventArgs<Player, string> item)
    {
        networkController.OnPlayerMove(item.Value);
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
}
