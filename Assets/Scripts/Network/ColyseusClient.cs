using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
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

    //protected List<Player> players = new List<Player>();
    protected IndexedDictionary<string, Player> players = new IndexedDictionary<string, Player>();
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

    public async Task ConnectToServer(string serverEndpoint, string username)
    {
        networkController = GetComponent<NetworkController>();
        if (!connecting && !IsConnected)
        {
            connecting = true;
            networkController.ServerStatusMessage = "Connecting...";
            Debug.Log("Connecting to " + serverEndpoint);
            if (string.IsNullOrEmpty(localPlayerName)) localPlayerName = username;

            // Connect to Colyeus Server
            endpoint = serverEndpoint;
            Debug.Log("log in client");
            client = ColyseusManager.Instance.CreateClient(endpoint);
            await client.Auth.Login();

            // Update username
            client.Auth.Username = username;
            Debug.Log("joining room");
            networkController.ServerStatusMessage = "Joining Room...";
            await JoinRoom();
            connecting = false;
        }
    }   
    
    public async Task Disconnect()
    {
        if (IsConnected)
        {
            await LeaveRoom();
            if (players != null)    
            {
                players.Clear();
            }
            client.Auth.Logout();
            localPlayerName = "";
        }
        client = null;
        networkController.ServerStatusMessage = "Disconnected";
    }

    async Task JoinRoom()
    {
        // For now, join / create the same room by name - if this is an existing room then both players will be in the
        // same room. This will likely need more work later.
        room = await client.JoinOrCreate<State>(roomName, new Dictionary<string, object>()
        {
            { "username", localPlayerName }
        });

        Debug.Log("Joined room successfully.");

        room.State.players.OnAdd += OnPlayerAdd;
        room.State.players.OnRemove += OnPlayerRemove;
        room.State.players.OnChange += OnPlayerChange;

        PlayerPrefs.SetString("roomId", room.Id);
        PlayerPrefs.SetString("sessionId", room.SessionId);
        PlayerPrefs.Save();

        room.OnStateChange += OnStateChangeHandler;
        room.OnMessage += OnMessage;
    }

    async Task LeaveRoom()
    {
        Debug.Log("closing connection");
        await room.Leave(true);
        room = null;
    }

    async Task GetAvailableRooms()
    {
        var roomsAvailable = await client.GetAvailableRooms(roomName);

        Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
        for (var i = 0; i < roomsAvailable.Length; i++)
        {
            Debug.Log("roomId: " + roomsAvailable[i].roomId);
            Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
            Debug.Log("clients: " + roomsAvailable[i].clients);
        }
    }
    public string GetClientList()
    {
        if (players != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Connected clients:");
            foreach (string p in players.Keys)
            {
                sb.AppendLine(p);
            }
            return sb.ToString();
        }
        else
        {
            return "";
        }
    }

    public Player GetPlayerById(string username)
    {
        return players[username];
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
        Debug.Log(msg);
    }

    void OnStateChangeHandler (State state, bool isFirstState)
    {
        // Setup room first state
        // This is where we might capture current state and save/load
        Debug.Log(state);
    }

    void OnPlayerAdd(Player player, string key)
    {
        Debug.Log("ColyseusClient - Player Add: " + player.username + " " + player.id + " key: " + key);
        bool isLocal = key == room.SessionId;
        players[player.username] = player;
        if (isLocal)
        {
            localPlayer = player;
            networkController.ServerStatusMessage = "Connected as " + player.username;
        }
        networkController.OnPlayerAdd(player);
    }

    void OnPlayerRemove(Player player, string key)
    {
        if (players[player.username] != null ) players.Remove(player.username);
        networkController.OnPlayerRemove(player);
    }

    void OnPlayerChange(Player player, string key)//(object sender, KeyValueEventArgs<Player, string> item)
    {
        Debug.Log(player + " " + key);
        networkController.OnPlayerChange(player);
    }

    public async void SendMovement(Vector3 pos, Quaternion rot )
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            Debug.Log(pos + " " + rot);
            t.position = new NetworkVector3 { x = pos.x, y = pos.y, z = pos.z };
            Vector3 r = rot.eulerAngles;
            t.rotation = new NetworkVector3 { x = r.x, y = r.y, z = r.z };
            await room.Send(new
            {
                transform = t,
                message = "movement"
            });
        }
    }
    public async void SendInteraction(Vector3 pos, Quaternion rot, Color color)
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            Debug.Log(pos + " " + rot);
            t.position = new NetworkVector3 { x = pos.x, y = pos.y, z = pos.z };
            Vector3 r = rot.eulerAngles;
            t.rotation = new NetworkVector3 { x = r.x, y = r.y, z = r.z };
            await room.Send(new
            {
                transform = t,
                color = color.ToString(),
                message = "interaction"
            });
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
