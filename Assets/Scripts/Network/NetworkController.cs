using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GameDevWare.Serialization;
using UnityEngine.SceneManagement;
using System.Text;

[RequireComponent(typeof(ColyseusClient))]
public class NetworkController : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public Button connectButton;
    public TMPro.TMP_Text connectButtonText;
    public InputField m_EndpointField;
    public Text connectionStatusText;
    private string _connectionStatusMessage;

    public GameObject avatar;
    private Player localPlayer;
    private GameObject localPlayerAvatar;
    public GameObject interactionIndicator;

    // todo: set up 4-digit pins for a collab room
    public string roomName = "ceasar";

    protected ColyseusClient colyseusClient;

    protected IndexedDictionary<Player, GameObject> players = new IndexedDictionary<Player, GameObject>();

    public TMPro.TMP_Text debugMessages;

    private string localPlayerName = "";

    private float lastUpdate;

    public bool autoConnect = false;
    public List<string> scenesWithAvatars;

    public bool IsConnected
    {
        get { return colyseusClient != null && colyseusClient.IsConnected; }
    }

    public string ServerStatusMessage
    {
        set
        {
            _connectionStatusMessage = value;
            if (connectionStatusText) connectionStatusText.text = value;
            if (connectButtonText && IsConnected) connectButtonText.text = "Disconnect";
            if (connectButtonText && !IsConnected) connectButtonText.text = "Connect";
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

        if (autoConnect)
        {
            ConnectToServer();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);
        if (IsConnected && scenesWithAvatars.Contains(scene.name))
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
        if (!IsConnected)
        {
            if (string.IsNullOrEmpty(localPlayerName))
            {
                SimulationManager manager = SimulationManager.GetInstance();
                localPlayerName = manager.GenerateUsername();
                manager.LocalPlayerColor = manager.GetColorForUsername(localPlayerName);
                Debug.Log(localPlayerName);
            }
            string _localEndpoint = "ws://localhost:2567/";
            string _remoteEndpoint = "wss://calm-meadow-14344.herokuapp.com";
#if UNITY_EDITOR
            string endpoint = _localEndpoint;
#else
            string endpoint = _remoteEndpoint; 
#endif
            // allow user to specify an endpoint
            endpoint = string.IsNullOrEmpty(m_EndpointField.text) ? endpoint : m_EndpointField.text;
            colyseusClient.ConnectToServer(endpoint, localPlayerName);
        }
        else if (IsConnected) Disconnect();
        else
        {
            Debug.Log("Already disconnected");
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
        Debug.Log("Player add! x => " + player.x + ", y => " + player.y + " playerName:" + player.username + " playerId: " + player.id);

        Vector3 pos = new Vector3(player.x, player.y, 0);
        if (isLocal)
        {
            localPlayer = player;
            localPlayerAvatar = Instantiate(avatar, pos, Quaternion.identity);
            Color playerColor = SimulationManager.GetInstance().LocalPlayerColor;
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


    public void OnPlayerChange(string playerId, Player updatedPlayer)
    {
        bool isLocal = playerId == localPlayer.id;
        bool isKnownPlayer = players.Keys.Contains(updatedPlayer);   // ToList().First(p => p.id == playerId);
        string knownPlayerName = isKnownPlayer ? updatedPlayer.username : "unknown";

        Debug.Log("player id " + playerId + " is local: " + isLocal + " isKnown: " + knownPlayerName);

        Player knownPlayer = players.Keys.ToList().Find(p => p.id == playerId);

        //bool interactionUpdate = isKnownPlayer && Utils.CompareNetworkTransform(updatedPlayer.interactionTarget, knownPlayer.interactionTarget);
        //bool movementUpdate = isKnownPlayer && Utils.CompareNetworkTransform(updatedPlayer.playerPosition, knownPlayer.playerPosition);

        //Debug.Log(interactionUpdate + " " + movementUpdate);

        GameObject remotePlayerAvatar;
        players.TryGetValue(updatedPlayer, out remotePlayerAvatar);
        if (isLocal)
        {
            Debug.Log("self update");
        }
        else if (knownPlayer != null)
        {
            //if (interactionUpdate)
            //{
            Debug.Log("Interaction update: " + updatedPlayer.interactionTarget.position.x + "," + updatedPlayer.interactionTarget.position.y + "," + updatedPlayer.interactionTarget.position.z);
            Vector3 pos = Utils.NetworkPosToPosition(updatedPlayer.interactionTarget.position);
            Quaternion rot = Utils.NetworkRotToRotation(updatedPlayer.interactionTarget.rotation);
            ShowInteraction(pos, rot, SimulationManager.GetInstance().GetColorForUsername(updatedPlayer.username), false);
            //}
            //else if (movementUpdate)
            //{
            Debug.Log("Movement update: " + updatedPlayer.playerPosition);
            if (remotePlayerAvatar)
            {

                remotePlayerAvatar.transform.Translate(new Vector3(updatedPlayer.playerPosition.position.x, updatedPlayer.playerPosition.position.y, 0));
            }
            else
            {
                localPlayerAvatar.transform.Translate(new Vector3(updatedPlayer.playerPosition.position.x, updatedPlayer.playerPosition.position.y, 0));
            }
            //}
        }

    }

    public void ShowInteraction(Vector3 pos, Quaternion rot, Color playerColor, bool isLocal)
    {
        if (interactionIndicator)
        {
            GameObject indicatorObj = Instantiate(interactionIndicator);
            indicatorObj.transform.localRotation = rot;
            indicatorObj.transform.position = pos;
            Utils.SetObjectColor(indicatorObj, playerColor);
            StartCoroutine(selfDestruct(indicatorObj));
        }
        if (isLocal)
        {
            // now need to broadcast to remotes
            colyseusClient.SendInteraction(pos, rot, playerColor);

        }

    }

    IEnumerator selfDestruct(GameObject indicatorObj)
    {
        yield return new WaitForSeconds(3.0f);
        if (indicatorObj)
        {
            Destroy(indicatorObj);
        }
    }
    // called when the game is terminated
    void OnDisable()
    {
        Debug.Log("OnDisable");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}