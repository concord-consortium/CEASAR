using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
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
    private Player localPlayer;
    private string localPlayerName = "";

    private float lastUpdate;
    private bool connecting = false;
    public bool IsConnected
    {
        get { return client != null && room != null; }
    }
    private string endpoint;
    private float heartbeatInterval = 10;

    private int retries = 0;
    private int maxRetries = 3;
    private float retryInterval = 1.0f;

    private IEnumerator clientConnectionCoroutine;

    private void Update()
    {
        if (client != null && localPlayer != null)
        {
            lastUpdate += Time.deltaTime;
            if (lastUpdate > heartbeatInterval)
            {
                // send update
                room.Send(new Dictionary<string, object>()
                    {
                        {"posX", localPlayer.x },
                        {"posY", localPlayer.y },
                        {"message", "heartbeat"}
                    }
                );
                lastUpdate = 0;
            }
        }
    }
//    IEnumerator ListenToServer()
//    {
//        while (true)
//        {
//            if (client != null)
//            {
//                /* Always call Recv if Colyseus connection is open */
//                client.Recv();
//            }
//            yield return 0;
//        }
//    }

    public async void ConnectToServer(string serverEndpoint, string username)
    {
        networkController = GetComponent<NetworkController>();
        if (!connecting && (!IsConnected || string.IsNullOrEmpty(localPlayerName)))
        {
//            if (clientConnectionCoroutine == null) clientConnectionCoroutine = ListenToServer();
//            try
//            {
//                StopCoroutine(clientConnectionCoroutine);
//            }
//            finally
//            {
//                StartCoroutine(clientConnectionCoroutine);
//            }

            connecting = true;
            networkController.ServerStatusMessage = "Connecting...";
            Debug.Log("Connecting to " + serverEndpoint);
            if (string.IsNullOrEmpty(localPlayerName)) localPlayerName = username;

            // Connect to Colyeus Server
            endpoint = serverEndpoint;
            client = ColyseusManager.Instance.CreateClient(endpoint);

            //await client.Auth.Login();
            //var friends = await client.Auth.GetFriends();

            //// Update username
            //client.Auth.Username = "MyUsername";
            //await client.Auth.Save();

            //client.OnOpen += (object sender, EventArgs e) =>
            //{
                Debug.Log("joining room");
                networkController.ServerStatusMessage = "Joining Room...";
                JoinRoom();
            //};
            /*
             * TODO: reinstate error handling on connections

            client.OnError += (sender, e) =>
            {
                Debug.LogError(e.Message);
                networkController.ServerStatusMessage = "Connection error! Attempting to fix...";
                string oldName = localPlayerName;
                Disconnect();
                StartCoroutine(ConnectRetry(endpoint, oldName));
            };
            client.OnClose += (sender, e) => Debug.Log("CONNECTION CLOSED");
            StartCoroutine(client.Connect());
            */
        }

    }
//    IEnumerator ConnectRetry(string endpoint, string name)
//    {
//        yield return new WaitForSeconds(retryInterval);
//        if (retries < maxRetries)
//        {
//            // try to reconnect
//            ConnectToServer(endpoint, name);
//            retries++;
//        }
//    }
    public void Disconnect()
    {
        if (IsConnected)
        {
            LeaveRoom();
            localPlayerName = "";
            if (players != null)
            {
                players.Clear();
            }
        }
        if (client != null)
        {
            // closing client connection
            //client.Close();
            connecting = false;
            client = null;
        }
        try
        {
            StopCoroutine(clientConnectionCoroutine);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Attempted to close Network connection but encountered an issue: " + e.Message);
        }
        networkController = GetComponent<NetworkController>();
        networkController.ServerStatusMessage = "Disconnected.";
    }

    async void JoinRoom()
    {
        bool canJoinExisting = false;
        string availableRoomID = "";
        var roomsAvailable = await client.GetAvailableRooms(roomName);
        //client.GetAvailableRooms(roomName, (RoomAvailable[] roomsAvailable) =>
        //{
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


        //});
        string roomToJoin = canJoinExisting ? availableRoomID : roomName;
        room = await client.Join<State>(roomToJoin, new Dictionary<string, object>()
        {
            { "username", localPlayerName }
        });

//        room.OnReadyToConnect += (sender, e) =>
//        {
//            Debug.Log("Ready to connect to room!");
//            StartCoroutine(room.Connect());
//        };
//        room.OnError += (sender, e) =>
//        {
//            Debug.LogError(e.Message);
//            networkController = GetComponent<NetworkController>();
//            networkController.ServerStatusMessage = "Connection error! Attempting to fix...";
//            string oldName = localPlayerName;
//            Disconnect();
//            StartCoroutine(ConnectRetry(endpoint, oldName));
//        };
        //room.OnJoin += (sender, e) =>
        //{
            Debug.Log("Joined room successfully.");

            room.State.players.OnAdd += OnPlayerAdd;
            room.State.players.OnRemove += OnPlayerRemove;
            room.State.players.OnChange += OnPlayerChange;

            PlayerPrefs.SetString("sessionId", room.SessionId);
            PlayerPrefs.Save();
        //};

        room.OnStateChange += OnStateChangeHandler;
        room.OnMessage += OnMessage;
    }

    async void ReJoinRoom()
    {
        string sessionId = PlayerPrefs.GetString("sessionId");
        if (string.IsNullOrEmpty(sessionId))
        {
            Debug.Log("Cannot ReJoin without having a sessionId");
            return;
        }

        room = await client.Reconnect<State>(roomName, sessionId);

        //room.OnReadyToConnect += (sender, e) =>
        //{
            //Debug.Log("Ready to connect to room!");
            //StartCoroutine(room.Connect());
        //};
        /*
        room.OnError += (sender, e) => Debug.LogError(e.Message);
        room.OnJoin += (sender, e) =>
        {
            Debug.Log("Joined room successfully.");
            room.State.players.OnAdd += OnPlayerAdd;
            room.State.players.OnRemove += OnPlayerRemove;
            room.State.players.OnChange += OnPlayerChange;
        };
        */

        room.OnStateChange += OnStateChangeHandler;
        room.OnMessage += OnMessage;
    }

    async void LeaveRoom()
    {
        Debug.Log("closing connection");
        if (room != null)
        {
            await room.Leave(false);
        }

    }

    async void GetAvailableRooms()
    {
        var roomsAvailable = await client.GetAvailableRooms(roomName);

        Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
        for (var i = 0; i < roomsAvailable.Length; i++)
        {
            Debug.Log("roomId: " + roomsAvailable[i].roomId);
            Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
            Debug.Log("clients: " + roomsAvailable[i].clients);
            Debug.Log("metadata: " + roomsAvailable[i].metadata);
        }
    }
    public string GetClientList()
    {
        if (players != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Connected clients:");
            foreach (Player p in players)
            {
                sb.AppendLine(p.username);
            }
            return sb.ToString();
        }
        else
        {
            return "";
        }
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

    void OnMessage(object msg)
    {
        var message = (IndexedDictionary<string, object>)msg;
        Debug.Log(message);
    }

    void OnStateChangeHandler (State state, bool isFirstState)
    {
        // Setup room first state
        // This is where we might capture current state and save/load
    }

    void OnPlayerAdd(Player player, string key)
    {
        if (!players.Contains(player)) players.Add(player);
        if (key == room.SessionId)
        {
            localPlayer = player;
            networkController.ServerStatusMessage = "Connected as " + player.username;
        }
        networkController.OnPlayerAdd(player, key == room.SessionId);
    }

    void OnPlayerRemove(Player player, string key)
    {
        if (players.Contains(player)) players.Remove(player);
        networkController.OnPlayerRemove(player);
    }

    void OnPlayerChange(Player player, string key)//(object sender, KeyValueEventArgs<Player, string> item)
    {
        Debug.Log(player + " " + key);
        networkController.OnPlayerChange(key, player);
    }

    public void SendMovement(NetworkTransform t)
    {
        if (IsConnected)
        {
            room.Send(new Dictionary<string, object>()
                    {
                        {"transform", t },
                        {"message", "movement"}
                    }
                    );
        }
    }
    public void SendInteraction(Vector3 pos, Quaternion rot, Color color)
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            t.position = new NetworkVector3 { x = pos.x, y = pos.y, z = pos.z };
            Vector3 r = rot.eulerAngles;
            t.rotation = new NetworkVector3 { x = r.x, y = r.y, z = r.z };
            room.Send(new Dictionary<string, object>()
                    {
                        {"transform", t },
                        {"color", color.ToString()},
                        {"message", "interaction"}
                    }
                    );
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
