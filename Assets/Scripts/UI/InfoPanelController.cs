using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text;
using SunCalcNet;

public class InfoPanelController : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public GameObject pushPinDetailsText;
    public GameObject constellationText;
    public GameObject networkUserList;
    public GameObject networkUserPrefab;
    public GameObject networkStatusText;
    public GameObject networkGroupText;

    private enum TextDisplayObject {
        None,
        Sun,
        Moon,
        Star
    }
    TextDisplayObject textDisplayObject = TextDisplayObject.None;

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
        events.StarSelected.AddListener(starSelectedText);
        events.SunSelected.AddListener(sunSelectedText);
        events.MoonSelected.AddListener(moonSelectedText);
        events.PlayerJoined.AddListener(playerListChanged);
        events.PlayerLeft.AddListener(playerListChanged);
        events.NetworkConnection.AddListener(connectionStatusUpdated);
        events.SimulationTimeChanged.AddListener(simulationTimeChanged);
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
        StringBuilder details = new StringBuilder();
        details.Append(manager.CurrentSimulationTime.ToShortDateString())
            .Append(" ")
            .Append(manager.CurrentSimulationTime.ToString("HH:mm"))
            .Append(" UTC");
        details.AppendLine();
        details.Append(manager.CurrentLocationDisplayName);
        pushPinDetailsText.GetComponent<TextMeshProUGUI>().SetText(details.ToString());

    }

    private void starSelectedText(Star starData)
    {
        StringBuilder description = new StringBuilder();
        if (starData == null)
        {
            description.Append("Now showing constellation: ");
            description.Append(manager.CurrentlySelectedConstellation);
            constellationText.GetComponent<TextMeshProUGUI>().SetText(description.ToString());
        }
        else
        {
            double longitudeTimeOffset = manager.CurrentLatLng.Longitude/15d;
            double lst = manager.CurrentSimulationTime.ToSiderealTime() + longitudeTimeOffset;
            AltAz altAz = Utils.CalculateAltitudeAzimuthForStar(starData.RA, starData.Dec,
                lst, manager.CurrentLatLng.Latitude);
            description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
            description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
            description.Append("Alt/Az: ")
                .Append(altAz.Altitude.ToString("F2"))
                .Append(", ")
                .Append(altAz.Azimuth.ToString("F2"))
                .Append("  Mag: ")
                .AppendLine(starData.Mag.ToString());
            description.Append("R.A.: ")
                .Append(starData.RA.ToString("F2"))
                .Append("  Dec: ")
                .Append(starData.Dec.ToString("F2"))
                // A value of 10000000 indicates missing or dubious (e.g., negative) parallax data in Hipparcos
                .Append(starData.Dist != 10000000 ?  "  Dist: " + (starData.Dist * 3.262f).ToString("F0") + "ly": "");
            constellationText.GetComponent<TextMeshProUGUI>().SetText(description.ToString());
        }
        textDisplayObject = TextDisplayObject.Star;
    }

    private void sunSelectedText(bool selected)
    {
        var solarPosition = SunCalc.GetSunPosition(manager.CurrentSimulationTime, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);
        StringBuilder description = new StringBuilder();
        description.AppendLine("The Sun");
        description.Append("Alt/Az: ")
            .Append(solarPosition.Altitude.ToString("F2"))
            .Append(", ")
            .AppendLine(solarPosition.Azimuth.ToString("F2"));
        constellationText.GetComponent<TextMeshProUGUI>().SetText(description.ToString());
        textDisplayObject = TextDisplayObject.Sun;
    }

    private void moonSelectedText(bool selected)
    {
        var lunarPosition = MoonCalc.GetMoonPosition(manager.CurrentSimulationTime, manager.CurrentLatLng.Latitude, manager.CurrentLatLng.Longitude);
        StringBuilder description = new StringBuilder();
        description.AppendLine("The Moon");
        description.Append("Alt/Az: ")
            .Append(lunarPosition.Altitude.ToString("F2"))
            .Append(", ")
            .AppendLine(lunarPosition.Azimuth.ToString("F2"));
        constellationText.GetComponent<TextMeshProUGUI>().SetText(description.ToString());
        textDisplayObject = TextDisplayObject.Moon;
    }

    private void simulationTimeChanged() {
        if (textDisplayObject == TextDisplayObject.Sun)
        {
            sunSelectedText(true);
        }
        if (textDisplayObject == TextDisplayObject.Moon)
        {
            moonSelectedText(true);
        }
    }

    private void connectionStatusUpdated(bool isConnected)
    {
        CCDebug.Log("Connection Status: " + isConnected, LogLevel.Info, LogMessageCategory.Networking);
        string status = isConnected ? "Connected as " + manager.LocalUsername : "Not connected";
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
        addPlayerToList(manager.LocalUsername);

        // update network players
        foreach (Player p in manager.AllRemotePlayers)
        {
            addPlayerToList(p.Name);
        }
#if UNITY_EDITOR
        if (manager.AllRemotePlayers.Length < 7)
        {
            int difference = 7 - manager.AllRemotePlayers.Length;
            for (int i = 0; i < difference; i++)
            {
                addPlayerToList("GreenTestuser1");
            }
        }
#endif
    }

    private void addPlayerToList(string playerName)
    {
        GameObject networkUserObj = Instantiate(networkUserPrefab);
        networkUserObj.transform.SetParent(networkUserList.transform, false);
        networkUserObj.transform.localPosition = Vector3.zero;
        networkUserObj.transform.localScale = Vector3.one;
        networkUserObj.GetComponent<TMP_Text>().text = playerName;
        networkUserObj.GetComponent<TMP_Text>().color = UserRecord.GetColorForUsername(playerName);
        networkUserObj.name = playerName;
    }

    private void removeAllListeners()
    {
        Debug.Log("removing listeners ");
        events.PushPinSelected.RemoveListener(updatePushpinText);
        events.StarSelected.RemoveListener(starSelectedText);
        events.SunSelected.RemoveListener(sunSelectedText);
        events.MoonSelected.RemoveListener(moonSelectedText);
        events.PlayerJoined.RemoveListener(playerListChanged);
        events.PlayerLeft.RemoveListener(playerListChanged);
        events.NetworkConnection.RemoveListener(connectionStatusUpdated);
        events.SimulationTimeChanged.RemoveListener(simulationTimeChanged);
    }
    private void OnDisable()
    {
        // removeAllListeners();
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }

}
