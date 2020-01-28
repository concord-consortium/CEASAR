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

    private List<GameObject> playerList;

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
        playerList = new List<GameObject>();
        connectButtons();
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

    public void ClearPlayers()
    {
        foreach(GameObject playertext in playerList)
        {
            Destroy(playertext);
        }
        playerList.Clear();
    }

    public void AddPlayer(string name)
    {
        if(playerNamePrefab != null)
        {
            GameObject playerText = Instantiate(playerNamePrefab);
            playerText.GetComponent<RectTransform>().SetParent(usersPanel.transform);
            TMPro.TextMeshProUGUI label = playerText.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            label.text = name;
            label.color = UserRecord.GetColorForUsername(name);
            playerList.Add(playerText);
        }
    }
}
