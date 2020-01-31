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
using JetBrains.Annotations;

public class ColyseusClient : MonoBehaviour
{
    public System.Random rng = new System.Random();

    protected Client client;
    protected Room<State> room;
    private NetworkController networkController;

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
                        {"message", "heartbeat"}
                    }
                );
                lastUpdate = 0;
            }
        }
    }

    public async Task ConnectToServer(string serverEndpoint, string username, string roomName)
    {
        networkController = GetComponent<NetworkController>();
        Debug.Log($"Connection: {!connecting}");
        Debug.Log($"Connection: {!IsConnected}");

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
            await JoinRoom(roomName);
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

    async Task JoinRoom(string roomName)
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

    async Task GetAvailableRooms(string roomName)
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
        if (players != null && players.ContainsKey(username))
        {
            return players[username];
        }
        else return null;
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
        if (msg is UpdateMessage)
        {
            // update messages have a message type and player Id we can use to update from remote interactions
            var m = (UpdateMessage) msg;
            Debug.Log(m.updateType + " " + m.playerId);
            Player player = players.Values.First(p => p.id == m.playerId);
            if (m.updateType == "deleteannotation")
            {
                networkController.HandleAnnotationDelete(player, m.metadata);
            }
            else
            {
                networkController.HandleNetworkInteraction(player, m.updateType);
            }
        }
        else
        {
            // unknown message type
            Debug.Log(msg);
        }
    }

    void OnStateChangeHandler (State state, bool isFirstState)
    {
        // Setup room first state
        // This is where we might capture current state and save/load
        // Debug.Log(state);
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

    void OnPlayerChange(Player player, string key)
    {
        Debug.Log(player + " " + key);
        networkController.OnPlayerChange(player);
    }
    
    public async void SendNetworkTransformUpdate(Vector3 pos, Quaternion rot, Vector3 scale, string transformName, string messageType)
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            Debug.Log(pos + " " + rot);
            t.position = new NetworkVector3 { x = pos.x, y = pos.y, z = pos.z };
            Vector3 r = rot.eulerAngles;
            t.rotation = new NetworkVector3 { x = r.x, y = r.y, z = r.z };
            t.localScale = new NetworkVector3 {x = scale.x, y = scale.y, z = scale.z};
            t.name = transformName;
            await room.Send(new
            {
                transform = t,
                message = messageType
            });
        }
    }

    public async void SendAnnotationDelete(string annotationName)
    {
        if (IsConnected)
        {
            await room.Send(new
            {
                annotationName = annotationName,
                message = "deleteannotation"
            });
        }
    }
    
    public async void SendCelestialInteraction(NetworkCelestialObject celestialObj)
    {
        if (IsConnected)
        {
            NetworkCelestialObject c = celestialObj;
            Debug.Log("celestial object" +  c);
            await room.Send(new
            {
                celestialObject = c,
                message = "celestialinteraction"
            });
        }
    }

    public async void SendPinUpdate(float latitude, float longitude, DateTime dateTime, Vector3 cameraRotationEuler)
    {
        if (IsConnected)
        {
            NetworkPerspectivePin pin = new NetworkPerspectivePin();
            pin.datetime = (float)dateTime.ToEpochTime();
            pin.latitude = latitude;
            pin.longitude = longitude;
            NetworkTransform t = new NetworkTransform();
            t.position = new NetworkVector3 { x = 0, y = 0, z = 0 };
            t.rotation = new NetworkVector3 { x = cameraRotationEuler.x, y = cameraRotationEuler.y, z = cameraRotationEuler.z };
            t.localScale = new NetworkVector3 {x = 1, y = 1, z = 1};
            t.name = "mainCamera";
            pin.cameraTransform = t;
            await room.Send(new
            {
                perspectivePin = pin,
                message = "locationpin"
            });
            Debug.LogWarning(pin.datetime);
        }
    }
    
    void OnApplicationQuit()
    {
        Disconnect();
    }
}
