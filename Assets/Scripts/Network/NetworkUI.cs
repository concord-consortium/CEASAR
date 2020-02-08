using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.ObjectModel;

public class NetworkUI : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public GameObject disconnectButton;
    public GameObject usersPanel;
    public GameObject localButton;
    public GameObject webButton;
    public GameObject devButton;
    public GameObject playerNamePrefab;
    public TMPro.TMP_Text disconnectButtonText;
    public TMPro.TMP_Text connectionStatusText;
    public TMPro.TMP_Text usernameText;
    public Image usernamePin;
    public TMPro.TMP_Text groupNameText;
    public TMPro.TMP_Text debugMessages;

    private Dictionary<string, GameObject> playerList;
    private bool isConnecting = false;
    private SimulationManager manager
    {
        get { return SimulationManager.GetInstance(); }
    }
    
    public string ConnectionStatusText {
        set {
            if (connectionStatusText)
            {
                connectionStatusText.text = value;
            }
        }
    }

    public string DebugMessage {
        set {
            debugMessages.text = value;
        }
    }

    public void RandomizeUsername()
    {
        manager.LocalPlayerRecord.Randomize();
        UserRecord user = manager.LocalPlayerRecord;
        usernameText.text = user.Username;
        usernamePin.color = user.color;
    }

    public void Start()
    {
        UserRecord user = manager.LocalPlayerRecord;
        usernameText.text = user.Username;
        usernamePin.color = user.color;
        groupNameText.text = user.group.FirstCharToUpper();
        groupNameText.color = Color.white;
        playerList = new Dictionary<string, GameObject>();
        connectButtons();
    }

    private void OnEnable()
    {
        // playerList = new Dictionary<string, GameObject>();
        SimulationEvents.GetInstance().PlayerJoined.AddListener(AddPlayer);
        SimulationEvents.GetInstance().PlayerLeft.AddListener(RemovePlayer);
    }

    private void OnDisable()
    {
        SimulationEvents.GetInstance().PlayerJoined.RemoveListener(AddPlayer);
        SimulationEvents.GetInstance().PlayerLeft.RemoveListener(RemovePlayer);
    }

    void Update()
    {

        if (isConnecting)
        {
            bool networkIsConnecting =
                manager.NetworkControllerComponent && manager.NetworkControllerComponent.IsConnecting;
            if (networkIsConnecting && localButton.activeInHierarchy)
            {
                localButton.SetActive(false);
                webButton.SetActive(false);
                devButton.SetActive(false);
            }
            // Only update UI while we're attempting to connect to hide the connection buttons and prevent reclicks
            if (!networkIsConnecting) isConnecting = false;
        } 
    }
    public void ConnectedStatusUpdate(bool isConnected)
    {
        if (isConnected)
        {
            disconnectButton.SetActive(true);
            localButton.SetActive(false);
            webButton.SetActive(false);
            devButton.SetActive(false);
        }
        else
        {
            disconnectButton.SetActive(false);
            localButton.SetActive(true);
            webButton.SetActive(true);
            devButton.SetActive(true);
        }
    }
    
    private void connectButtons()
    {
        if(localButton != null)
        {
            Button localButtonButton = localButton.GetComponent<Button>();
            localButtonButton.onClick.AddListener(() => HandleConnectClick(ServerList.Local));
        }
        if (devButton != null)
        {
            Button devButtonButton = devButton.GetComponent<Button>();
            devButtonButton.onClick.AddListener(() => HandleConnectClick(ServerList.Dev));
        }

        if (webButton != null)
        {
            Button webButtonButton = webButton.GetComponent<Button>();
            webButtonButton.onClick.AddListener(() => HandleConnectClick(ServerList.Web));
        }
        if (disconnectButton != null)
        {
            Button disconnectButtonButton = disconnectButton.GetComponent<Button>();
            disconnectButtonButton.onClick.AddListener(() => HandleDisconnect());
        }
    }


    public void HandleConnectClick(ServerRecord server)
    {
        NetworkController networkController = manager.NetworkControllerComponent;
        CCDebug.Log($"Connecting to #{server.name}", LogLevel.Info, LogMessageCategory.Networking);

        if (!networkController.IsConnected)
        {
            // Only allow connection attempt if we're not currently waiting on a connect operation
            if (!networkController.IsConnecting)
            {
                // monitor connection locally for UI update
                isConnecting = true;
                networkController.ConnectToServer(server.address);
            }
        }
        else
        {
            networkController.Disconnect();
        }
        
    }

    public void HandleDisconnect()
    {
        NetworkController networkController = manager.NetworkControllerComponent;
        networkController.Disconnect();
        // clear out other players from the network list
        foreach (string playerName in playerList.Keys)
        {
            RemovePlayer(playerName);
        }
    }

    public string Username {
        set { 
            usernameText.text = value;
            usernamePin.color = manager.LocalPlayerColor;
        }
    }


    private GameObject MakePlayerLabel(string name)
    {
        GameObject playerLabel = null;
        if (playerNamePrefab != null)
        {
            playerLabel = Instantiate(playerNamePrefab);
            RectTransform buttonTransform = playerLabel.GetComponent<RectTransform>();
            buttonTransform.SetParent(usersPanel.transform);
            buttonTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            buttonTransform.localPosition = new Vector3(buttonTransform.localPosition.x, buttonTransform.localPosition.y, 0);
            buttonTransform.localRotation = Quaternion.identity;
            TMPro.TextMeshProUGUI label = playerLabel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Image pin = playerLabel.transform.Find("pin").GetComponent<Image>();
            label.text = name;
            pin.color = UserRecord.GetColorForUsername(name);
            Button b = playerLabel.GetComponent<Button>();
            b.onClick.AddListener( () => PlayerLabelClicked(name));
        }
        // Return null if it didn't work...
        return playerLabel;
    }

    public void AddPlayer(string name)
    {
        CCDebug.Log($"Player ADDED to panel #{name}", LogLevel.Verbose, LogMessageCategory.UI);
        if (playerList.ContainsKey(name))
        {
            if(playerList[name] == null)
            {
                playerList[name] = MakePlayerLabel(name);
            }
        }
        else
        {
            playerList.Add(name, MakePlayerLabel(name));
        }
    }

    public void RemovePlayer(string name)
    {
        CCDebug.Log($"Player REMOVED from panel #{name}", LogLevel.Verbose, LogMessageCategory.UI);
        if(playerList.ContainsKey(name))
        {
            Destroy(playerList[name]);
            playerList.Remove(name);
        }
        else
        {
            CCDebug.Log($"No Label for {name} found in roster", LogLevel.Error, LogMessageCategory.UI);
        }
    }

    private void PlayerLabelClicked(string username)
    {
        Player remotePlayer = manager.GetRemotePlayer(username);
        CCDebug.Log($"Push Pin clicked for {remotePlayer.Name} {remotePlayer.Pin}", LogLevel.Info, LogMessageCategory.UI);
        Pushpin pin = remotePlayer.Pin;
        
        manager.JumpToPin(pin);

        // Invoking this event will update the simulation time and location;
        SimulationEvents.GetInstance().PushPinSelected.Invoke(pin);

        // Switch to Horizon view
        if (SceneManager.GetActiveScene().name != SimulationConstants.SCENE_HORIZON)
        {
            SceneManager.LoadScene(SimulationConstants.SCENE_HORIZON);
        }
        else
        {
            Vector3 rot = remotePlayer.CameraDirection;
            // for desktop / non-VR:
            // the x component of the rotation goes on the Main Camera. The Y component goes on its parent. 

#if !UNITY_ANDROID
            Transform mainCameraTransform = Camera.main.transform;
            mainCameraTransform.rotation = Quaternion.Euler(rot.x, 0, 0);
            mainCameraTransform.parent.rotation = Quaternion.Euler(0, rot.y, 0);
#else
            GameObject vrCameraRig = GameObject.Find("VRCameraRig");
            if (vrCameraRig != null){
              vrCameraRig.transform.rotation = Quaternion.Euler(0, rot.y, 0);
            }
#endif
            
        }
        
    } 
}
