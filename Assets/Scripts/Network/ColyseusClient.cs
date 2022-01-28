using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

public enum NetworkMessageType
{
    Movement,
    Interaction,
    LocationPin,
    CelestialInteraction,
    Annotation,
    DeleteAnnotation,
    Heartbeat,
    Text
}
public class ColyseusClient : MonoBehaviour
{
    private Client client;
    private Room<RoomState> room;
    private NetworkController networkController;

    private IndexedDictionary<string, NetworkPlayer> players = new IndexedDictionary<string, NetworkPlayer>();
    private NetworkPlayer _localNetworkPlayer;
    private string localPlayerName = "";

    private float lastUpdate;
    private bool connecting = false;
    public bool IsConnecting
    {
        get { return connecting; }
    }
    public bool IsConnected
    {
        get { return client != null && room != null; }
    }
    private string endpoint;
    private float heartbeatInterval = 10;

    private void Update()
    {
        if (client != null && _localNetworkPlayer != null)
        {
            lastUpdate += Time.deltaTime;
            if (lastUpdate > heartbeatInterval)
            {
                // send update
                room.Send(NetworkMessageType.Heartbeat.ToString());

                lastUpdate = 0;
            }
        }
    }

    public void ConnectToServer(string serverEndpoint, string username, string roomName)
    {
        networkController = GetComponent<NetworkController>();
        CCDebug.Log($"Connect to Server called: isConnected: {IsConnected}, currently connecting: {!connecting} ", LogLevel.Verbose, LogMessageCategory.Networking);

        if (!connecting && !IsConnected)
        {
            connecting = true;
            networkController.ServerStatusMessage = "Connecting...";
            CCDebug.Log("Connecting to " + serverEndpoint);
            if (string.IsNullOrEmpty(localPlayerName)) localPlayerName = username;

            // Connect to Colyseus Server
            endpoint = serverEndpoint;
            CCDebug.Log("log in client", LogLevel.Verbose, LogMessageCategory.Networking);
            client = ColyseusManager.Instance.CreateClient(endpoint);

            try
            {
                CCDebug.Log("joining room", LogLevel.Verbose, LogMessageCategory.Networking);
                networkController.ServerStatusMessage = "Joining Room...";
                JoinRoom(roomName);
            }
            catch (Exception ex)
            {
                CCDebug.Log(ex, LogLevel.Error, LogMessageCategory.Networking);
                Disconnect();
                connecting = false;
                networkController.ServerStatusMessage = ex.Message;
            }
        }
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            LeaveRoom();
            if (players != null)
            {
                players.Clear();
            }
            // client.Auth.Logout();
            localPlayerName = "";
        }
        client = null;
        networkController.ServerStatusMessage = "";
    }

    async void JoinRoom(string roomName)
    {
        // For now, join / create the same room by name - if this is an existing room then both players will be in the
        // same room. This will likely need more work later.
        room = await client.JoinOrCreate<RoomState>(roomName, new Dictionary<string, object>()
        {
            { "username", localPlayerName }
        });

        CCDebug.Log("Joined room successfully.");
        connecting = false;
        registerRoomHandlers();
        SimulationEvents.Instance.NetworkConnection.Invoke(IsConnected);
    }

    void registerRoomHandlers()
    {
        room.State.players.OnAdd += OnPlayerAdd;
        room.State.players.OnRemove += OnPlayerRemove;
        room.State.players.OnChange += OnPlayerChange;
        // room.State.TriggerAll();

        PlayerPrefs.SetString("roomId", room.Id);
        PlayerPrefs.SetString("sessionId", room.SessionId);
        PlayerPrefs.Save();

        room.OnLeave += (code) => CCDebug.Log($"User leaving room: {code}");
        room.OnError += (code, message) => CCDebug.LogError("Network ERROR, code =>" + code + ", message => " + message);
        room.OnStateChange += OnStateChangeHandler;

        room.OnMessage((UpdateMessage message) =>
        {
            if (players.ContainsKey(message.playerId))
            {
                if (message.updateType == NetworkMessageType.DeleteAnnotation.ToString())
                {
                    networkController.HandleAnnotationDelete(GetPlayerById(message.playerId), message.metadata);
                }
                else
                {
                    networkController.HandleNetworkInteraction(GetPlayerById(message.playerId), message.updateType);
                }
            }
        });
    }

    public NetworkPlayer GetPlayerById(string playerId)
    {
        if (players != null && players.ContainsKey(playerId))
        {
            return players[playerId];
        }
        else return null;
    }

    void OnMessageReceived(UpdateMessage message)
    {
        CCDebug.Log("Received Schema Message!");
        CCDebug.Log(message);
        if (players.ContainsKey(message.playerId))
        {
            NetworkPlayer networkPlayer = players.Values.First(p => p.id == message.playerId);
            if (message.updateType == NetworkMessageType.DeleteAnnotation.ToString())
            {
                networkController.HandleAnnotationDelete(networkPlayer, message.metadata);
            }
            else
            {
                networkController.HandleNetworkInteraction(networkPlayer, message.updateType);
            }
        }
    }
    async void LeaveRoom()
    {
        CCDebug.Log("closing connection");
        await room.Leave(true);
        room = null;
    }

    void OnStateChangeHandler (RoomState state, bool isFirstState)
    {
        // unused
    }

    void OnPlayerAdd(NetworkPlayer networkPlayer, string key)
    {
        CCDebug.Log("ColyseusClient - Player Add: " + networkPlayer.username + " " + networkPlayer.id + " key: " + key, LogLevel.Info, LogMessageCategory.Networking);
        bool isLocal = key == room.SessionId;
        players[networkPlayer.id] = networkPlayer;
        if (isLocal)
        {
            _localNetworkPlayer = networkPlayer;
            networkController.ServerStatusMessage = "Connected as " + networkPlayer.username;
        }
        networkController.OnPlayerAdd(networkPlayer);
    }

    void OnPlayerRemove(NetworkPlayer networkPlayer, string key)
    {
        if (players[networkPlayer.id] != null ) players.Remove(networkPlayer.id);
        networkController.OnPlayerRemove(networkPlayer);
    }

    void OnPlayerChange(NetworkPlayer networkPlayer, string key)
    {
        networkController.OnPlayerChange(networkPlayer);
    }

    public async void SendNetworkTransformUpdate(Vector3 pos, Quaternion rot, Vector3 scale, string transformName, NetworkMessageType messageType)
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            t.position = new NetworkVector3 { x = pos.x, y = pos.y, z = pos.z };
            Vector3 r = rot.eulerAngles;
            t.rotation = new NetworkVector3 { x = r.x, y = r.y, z = r.z };
            t.localScale = new NetworkVector3 {x = scale.x, y = scale.y, z = scale.z};
            t.name = transformName;
            await room.Send(messageType.ToString(), t);
        }
    }

    public async void SendNetworkAnnotationUpdate(Vector3 startPos, Vector3 endPos, Vector3 rotation, string transformName, NetworkMessageType messageType)
    {
        if (IsConnected)
        {
            NetworkTransform t = new NetworkTransform();
            // This is a hack to use the NetworkTransform class to communicate the start/end positions
            // of the annotation. The NetworkTransform class is designed to store center, scale, and rotation,
            // but for now we will store the start and end positions of the annotation in the first and third vector3 slots.
            // Ideally we will clean this up and expand the network communication structures to handle the
            // annotations properly.
            t.position = new NetworkVector3 { x = startPos.x, y = startPos.y, z = startPos.z };
            t.rotation = new NetworkVector3 { x = rotation.x, y = rotation.y, z = rotation.z };
            t.localScale = new NetworkVector3 { x = endPos.x, y = endPos.y, z = endPos.z };
            t.name = transformName;
            await room.Send(messageType.ToString(), t);
        }
    }

    public async void SendAnnotationDelete(string annotationName)
    {
        if (IsConnected)
        {
            await room.Send(NetworkMessageType.DeleteAnnotation.ToString(), annotationName);
        }
    }

    public async void SendCelestialInteraction(NetworkCelestialObject celestialObj)
    {
        if (IsConnected)
        {
            NetworkCelestialObject c = celestialObj;
            CCDebug.Log("celestial object" +  c, LogLevel.Verbose, LogMessageCategory.Networking);
            await room.Send(NetworkMessageType.CelestialInteraction.ToString(), c);
        }
    }

    public async void SendPinUpdate(float latitude, float longitude, DateTime dateTime, Vector3 cameraRotationEuler, string locationName)
    {
        if (IsConnected)
        {
            NetworkPerspectivePin pin = new NetworkPerspectivePin();
            pin.datetime = (float)dateTime.ToEpochTime();
            pin.latitude = latitude;
            pin.longitude = longitude;
            pin.locationName = locationName;
            NetworkTransform t = new NetworkTransform();
            t.position = new NetworkVector3 { x = 0, y = 0, z = 0 };
            t.rotation = new NetworkVector3 { x = cameraRotationEuler.x, y = cameraRotationEuler.y, z = cameraRotationEuler.z };
            t.localScale = new NetworkVector3 {x = 1, y = 1, z = 1};
            t.name = "mainCamera";
            pin.cameraTransform = t;
            await room.Send(NetworkMessageType.LocationPin.ToString(), pin);
        }
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
