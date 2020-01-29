using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public GameObject disconnectButton;
    public Button randomizeUsernameButton;
    public GameObject usersPanel;
    public GameObject smallOrangeButtonPrefab;
    public GameObject localButton;
    public GameObject webButton;
    public GameObject devButton;
    public GameObject playerNamePrefab;
    public TMPro.TMP_Text disconnectButtonText;
    public TMPro.TMP_Text connectionStatusText;
    public TMPro.TMP_Text usernameText;
    public TMPro.TMP_Text groupNameText;
    public TMPro.TMP_Text debugMessages;

    private Dictionary<string, GameObject> playerList;

    public string DisconnectButtonText {
        set {
            if (disconnectButtonText) disconnectButtonText.text = value;
        }
    }
    public string ConnectionStatusText {
        set {
            if (connectionStatusText) connectionStatusText.text = value;
        }
    }

    public string DebugMessage {
        set {
            debugMessages.text = value;
        }
    }

    public void RandomizeUsername()
    {
        SimulationManager.GetInstance().LocalPlayer.Randomize();
        UserRecord user = SimulationManager.GetInstance().LocalPlayer;
        usernameText.text = user.Username;
        usernameText.color = user.color;
    }

    public void Start()
    {
        UserRecord user = SimulationManager.GetInstance().LocalPlayer;
        usernameText.text = user.Username;
        usernameText.color = user.color;
        groupNameText.text = user.group;
        groupNameText.color = Color.white;
        playerList = new Dictionary<string, GameObject>();
        connectButtons();
    }

    private void OnEnable()
    {
        SimulationEvents.GetInstance().PlayerJoined.AddListener(AddPlayer);
        SimulationEvents.GetInstance().PlayerLeft.AddListener(RemovePlayer);
    }

    private void OnDisable()
    {
        SimulationEvents.GetInstance().PlayerJoined.RemoveListener(AddPlayer);
        SimulationEvents.GetInstance().PlayerLeft.RemoveListener(RemovePlayer);
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
        NetworkController networkController = FindObjectOfType<NetworkController>();
        Debug.Log($"Connecting to #{server.name}");

        if (!networkController.IsConnected)
        {
            networkController.ConnectToServer(server.address);
            if (randomizeUsernameButton != null) randomizeUsernameButton.enabled = false;
        }
        else
        {
            networkController.Disconnect();
            randomizeUsernameButton.enabled = true;
        }
        
    }

    public void HandleDisconnect()
    {
        NetworkController networkController = FindObjectOfType<NetworkController>();
        networkController.Disconnect();
        randomizeUsernameButton.enabled = true;
    }

    public string Username {
        set { 
            usernameText.text = value;
            usernameText.color = SimulationManager.GetInstance().LocalPlayerColor;
        }
    }

    //public void RepaintPlayerList()
    //{
    //    // Remove them all first:
    //    foreach(string name in playerList.Keys)
    //    {
    //        Destroy(playerList[name]);
    //    }
    //    // Add them all:
    //}

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
            TMPro.TextMeshProUGUI label = playerLabel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Image pin = playerLabel.transform.Find("pin").GetComponent<Image>();
            label.text = name;
            label.color = UserRecord.GetColorForUsername(name);
            pin.color = UserRecord.GetColorForUsername(name);
            Button b = playerLabel.GetComponent<Button>();
            b.onClick.AddListener( () => PlayerLabelClicked(name));
        }
        // Return null if it didn't work...
        return playerLabel;
    }

    public void AddPlayer(string name)
    {
        Debug.Log($"Player ADDED to panel #{name}");
        if (playerList.ContainsKey(name))
        {
            Debug.Log($"Player {name} exists already");
            if(playerList[name] == null)
            {
                Debug.Log($"Player {name} needs a label; making one");
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
        Debug.Log($"Player REMOVED from panel #{name}");
        if(playerList.ContainsKey(name))
        {
            Destroy(playerList[name]);
            playerList.Remove(name);
        }
        else
        {
            Debug.LogError($"No Label for {name} found in roster");
        }
    }

    private void PlayerLabelClicked(string username)
    {
        // TODO: Look up the user, see if they have a locaation pin
        // If they do, open the horizon view at that pin location.
        Debug.Log($"{username} was clicked");
    } 
}
