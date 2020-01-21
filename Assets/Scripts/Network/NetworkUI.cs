using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    // UI Buttons are attached through Unity Inspector
    public Button connectButton;
    public Button randomizeUsernameButton;
    public GameObject roomButtonPanel;
    public GameObject smallOrangeButtonPrefab;
    public TMPro.TMP_Text connectButtonText;
    public TMPro.TMP_InputField m_EndpointField;
    public TMPro.TMP_Text connectionStatusText;
    public TMPro.TMP_Text usernameText;
    public TMPro.TMP_Text debugMessages;

    public string ConnectButtonText {
        set {
            if (connectButtonText) connectButtonText.text = value;
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
        NetworkController networkController = FindObjectOfType<NetworkController>();
        networkController.RandomizeUsername();
    }
    public void SetNetworkAddress(string address)
    {
        NetworkController networkController = FindObjectOfType<NetworkController>();
        switch (address)
        {
            case "local":
                networkController.SetNetworkAddress(NetworkConnection.Local);
                break;
            case "dev":
                networkController.SetNetworkAddress(NetworkConnection.Dev);
                break;
            default:
                networkController.SetNetworkAddress(NetworkConnection.Remote);
                break;
        }
    }
    public string NetworkAddress {
        get {
            return m_EndpointField.text;
        }
        set {
            m_EndpointField.text = value;
        }
    }

    public void HandleConnectClick()
    {
        NetworkController networkController = FindObjectOfType<NetworkController>();
        if (!networkController.IsConnected)
        {
            networkController.ConnectToServer(m_EndpointField.text);
            if (randomizeUsernameButton != null) randomizeUsernameButton.enabled = false;

        }
        else
        {
            networkController.Disconnect();
            randomizeUsernameButton.enabled = true;
        }
        
    }

    private void Start()
    {
        foreach (string name in NetworkController.roomNames)
        {
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(roomButtonPanel.transform);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();
            if(label != null )
            {
                label.SetText(name);
            }
            Debug.Log("Label is " + label);
            if(b != null)
            {
                b.onClick.AddListener(() => handleRoomClick(name));
            }
        }
    }

    public void handleRoomClick(string name)
    {
        NetworkController networkController = FindObjectOfType<NetworkController>();
        networkController.roomName = name;
        Debug.Log(name);
    }

    public string Username {
        set { 
            usernameText.text = value;
            usernameText.color = SimulationManager.GetInstance().LocalPlayerColor;
        }
    }
}
