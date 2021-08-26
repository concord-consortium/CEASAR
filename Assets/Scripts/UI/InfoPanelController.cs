using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text;

public class InfoPanelController : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject locationDetailsText;
    public GameObject dateTimeDetailsText;
    public GameObject networkUserList;
    public GameObject networkUserPrefab;
    public GameObject networkStatusText;
    public GameObject networkGroupText;

    public TextMeshProUGUI debugText;

    private string lastDebugMessage = "";
    private void Awake()
    {
        DontDestroyOnLoad(this.transform.root.gameObject);
        manager.InfoPanel = this;
    }

    private void Start()
    {
        Debug.Log("wiring up event listeners ");
        Debug.Log(events.PushPinSelected.GetPersistentEventCount());
        events.PushPinSelected.AddListener(updatePushpinText);
        events.PlayerJoined.AddListener(playerListChanged);
        events.PlayerLeft.AddListener(playerListChanged);
        events.NetworkConnection.AddListener(connectionStatusUpdated);
        playerListChanged(manager.LocalUsername);
        updatePushpinText(manager.LocalPlayerPin);
    }
    private void Update()
    {
        if (debugText != null && lastDebugMessage != manager.LastLogMessageDebug)
        {
            debugText.SetText(manager.LastLogMessageDebug);
            lastDebugMessage = manager.LastLogMessageDebug;
        }
    }
    private void updatePushpinText(Pushpin pin)
    {
        StringBuilder dateTimeDetails = new StringBuilder();
        dateTimeDetails.Append(manager.CurrentSimulationTime.ToShortDateString())
            .Append(" ")
            .Append(manager.CurrentSimulationTime.ToString("HH:mm"))
            .Append(" UTC");
        dateTimeDetailsText.GetComponent<TextMeshProUGUI>().SetText(dateTimeDetails.ToString());

        StringBuilder locationDetails = new StringBuilder();
        locationDetails.Append(manager.CurrentLocationDisplayName);
        locationDetailsText.GetComponent<TextMeshProUGUI>().SetText(locationDetails.ToString());
    }

    private void connectionStatusUpdated(bool isConnected)
    {
        CCDebug.Log("Connection Status: " + isConnected, LogLevel.Info, LogMessageCategory.Networking);
        string status = isConnected ? "Connected" : "Not connected";
        networkStatusText.GetComponent<TextMeshProUGUI>().SetText(status);
        networkGroupText.GetComponent<TextMeshProUGUI>().SetText(manager.GroupName.FirstCharToUpper());
    }
    private void playerListChanged(string playerName)
    {
        // Clear the list
        foreach (Transform t in networkUserList.transform)
        {
            Debug.Log("destroying " + t.name);
            Destroy(t.gameObject);
        }
        // add local user
        addPlayerToList(manager.LocalUsername, manager.LocalPlayer);

        // update network players
        foreach (Player p in manager.AllRemotePlayers)
        {
            addPlayerToList(p.Name, p);
        }
#if UNITY_EDITOR
        if (manager.AllRemotePlayers.Length < 7)
        {
            int difference = 7 - manager.AllRemotePlayers.Length;
            for (int i = 0; i < difference; i++)
            {
                addPlayerToList("GreenTestuser1", null);
            }
        }
#endif
    }

    private void addPlayerToList(string playerName, Player player)
    {
        GameObject networkUserObj = Instantiate(networkUserPrefab);
        networkUserObj.transform.SetParent(networkUserList.transform, false);
        networkUserObj.transform.localPosition = Vector3.zero;
        networkUserObj.transform.localScale = Vector3.one;
        networkUserObj.GetComponent<GroupUserButton>().Setup(player, playerName, UserRecord.GetColorForUsername(playerName), manager.LocalUsername == playerName);
        networkUserObj.name = playerName;
    }

    public void ChangeYear(int yearChange)
    {
        // Setting simulation time updates Local User Pin
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddYears(yearChange);
        BroadcastTimeChange();
    }

    public void ChangeMonth(int monthChange)
    {
        // Setting simulation time updates Local User Pin
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddMonths(monthChange);
        BroadcastTimeChange();
    }

    public void ChangeDay(int dayChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddDays(dayChange);
         BroadcastTimeChange();
    }

    public void ChangeHour(int hourChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddHours(hourChange);
        BroadcastTimeChange();
    }

    public void ChangeMinute(int minuteChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddMinutes(minuteChange);
        BroadcastTimeChange();
    }

    public void BroadcastTimeChange()
    {
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
        // broadcast the update to remote players
        SimulationEvents.Instance.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
        events.SimulationTimeChanged.Invoke();
    }

    private void removeAllListeners()
    {
        Debug.Log("removing listeners ");
        events.PushPinSelected.RemoveListener(updatePushpinText);
        events.PlayerJoined.RemoveListener(playerListChanged);
        events.PlayerLeft.RemoveListener(playerListChanged);
        events.NetworkConnection.RemoveListener(connectionStatusUpdated);
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }

}
