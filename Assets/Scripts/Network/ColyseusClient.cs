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

    public async void ConnectToServer(string serverEndpoint, string username)
    {
        networkController = GetComponent<NetworkController>();
        if (!connecting && (!IsConnected || string.IsNullOrEmpty(localPlayerName)))
        {
            connecting = true;
            networkController.ServerStatusMessage = "Connecting...";
            Debug.Log("Connecting to " + serverEndpoint);
            if (string.IsNullOrEmpty(localPlayerName)) localPlayerName = username;

            // Connect to Colyeus Server
            endpoint = serverEndpoint;
            client = ColyseusManager.Instance.CreateClient(endpoint);
            await client.Auth.Login();
            
            // Update username
            client.Auth.Username = username;
            await client.Auth.Save();
            
            Debug.Log("joining room");
            networkController.ServerStatusMessage = "Joining Room...";
            JoinRoom();
        }
    }
    
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
        Debug.Log("Player Add: " + player.username + " " + player.id + " key: " + key);
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
