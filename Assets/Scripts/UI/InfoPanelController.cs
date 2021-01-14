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
    
    public GameObject pushPinDetailsText;
    public GameObject constellationText;
    public GameObject networkUserList;
    public GameObject networkUserPrefab;
    
    private void OnEnable()
    {
        events.PushPinUpdated.AddListener(updatePushpinText);
        events.StarSelected.AddListener(starSelectedText);
        events.PlayerJoined.AddListener(playerListChanged);
        events.PlayerLeft.AddListener(playerListChanged);
        events.NetworkConnection.AddListener(connectionStatusUpdated);
        playerListChanged(manager.LocalUsername);
        updatePushpinText(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
    }

    private void updatePushpinText(Pushpin pin, Vector3 lookDirection)
    {
        StringBuilder details = new StringBuilder();
        details.AppendLine(manager.CurrentSimulationTime.ToShortDateString())
            .Append(" ")
            .Append(manager.CurrentSimulationTime.ToShortTimeString());
        details.AppendLine(manager.CurrentLocationName);
        
        pushPinDetailsText.GetComponent<TextMeshProUGUI>().SetText(pin.ToString());   
        
    }
    
    private void starSelectedText(Star starData)
    {
        double longitudeTimeOffset = manager.CurrentLatLng.Longitude/15d;
        double lst = manager.CurrentSimulationTime.ToSiderealTime() + longitudeTimeOffset;
        AltAz altAz = Utils.CalculateAltitudeAzimuthForStar(starData.RA, starData.Dec,
            lst, manager.CurrentLatLng.Latitude);
        StringBuilder description = new StringBuilder();
        description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
        description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
        description.Append("Alt/Az: ")
            .Append(altAz.Altitude.ToString("F2"))
            .Append(", ")
            .Append(altAz.Azimuth.ToString("F2"))
            .Append("  Mag: ")
            .Append(starData.Mag.ToString());
        constellationText.GetComponent<TextMeshProUGUI>().SetText(description.ToString());
    }

    private void connectionStatusUpdated(bool isConnected)
    {
        CCDebug.Log("Connection Status: " + isConnected, LogLevel.Info, LogMessageCategory.Networking);
        // playerListChanged(manager.LocalUsername);
    }
    private void playerListChanged(string playerName)
    {
        // Clear the list
        foreach (Transform t in networkUserList.transform)
        {
            Destroy(t.gameObject);
        }
        // add local user
        addPlayerToList(manager.LocalUsername);
        
        // update network players
        foreach (Player p in manager.AllRemotePlayers)
        {
            addPlayerToList(p.Name);
        }
    }

    private void addPlayerToList(string playerName)
    {
        GameObject networkUserObj = Instantiate(networkUserPrefab);
        networkUserObj.transform.SetParent(networkUserList.transform, false);
        networkUserObj.transform.localPosition = Vector3.zero;
        networkUserObj.transform.localScale = Vector3.one;
        networkUserObj.GetComponent<TMP_Text>().text = playerName;
        networkUserObj.GetComponent<TMP_Text>().color = UserRecord.GetColorForUsername(playerName);
    }

    private void removeAllListeners()
    {
        events.PushPinUpdated.RemoveListener(updatePushpinText);
        events.StarSelected.RemoveListener(starSelectedText);
        events.PlayerJoined.RemoveListener(playerListChanged);
        events.PlayerLeft.RemoveListener(playerListChanged);
        events.NetworkConnection.RemoveListener(connectionStatusUpdated);
    }
    private void OnDisable()
    {
        removeAllListeners();
    }

    private void OnDestroy()
    {
       removeAllListeners();
    }
    
}
