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
    
    public TMP_Text pushPinDetailsText;
    public TMP_Text constellationText;
    public GameObject networkUserList;
    public GameObject networkUserPrefab;
    private void OnEnable()
    {
        events.PushPinUpdated.AddListener(updatePushpinText);
        events.StarSelected.AddListener(starSelectedText);
        events.PlayerJoined.AddListener(playerListChanged);
        events.PlayerLeft.AddListener(playerListChanged);
    }

    private void updatePushpinText(Pushpin pin, Vector3 lookDirection)
    {
        StringBuilder details = new StringBuilder();
        details.Append(pin.SelectedDateTime.ToShortDateString())
            .Append(" ")
            .Append(pin.SelectedDateTime.ToShortTimeString())
            .AppendLine(pin.Location.ToDisplayString());
        pushPinDetailsText.text = pin.ToString();
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
        constellationText.text =  description.ToString();
    }

    private void playerListChanged(string playerName)
    {
        foreach (Player p in manager.AllRemotePlayers)
        {
            GameObject networkUserObj = Instantiate(networkUserPrefab);
            networkUserObj.transform.parent = networkUserList.transform;
            networkUserObj.transform.localPosition = Vector3.zero;
            networkUserObj.transform.localScale = Vector3.one;
            networkUserObj.GetComponent<TMP_Text>().text = p.Name;
            networkUserObj.GetComponent<TMP_Text>().color = UserRecord.GetColorForUsername(p.Name);
        }

        for (int i = 0; i < 6; i++)
        {
            GameObject networkUserObj = Instantiate(networkUserPrefab);
            networkUserObj.transform.parent = networkUserList.transform;
            networkUserObj.transform.localPosition = Vector3.zero;
            networkUserObj.transform.localScale = Vector3.one;
            networkUserObj.GetComponent<TMP_Text>().text = manager.LocalUsername + "_" + i;
            networkUserObj.GetComponent<TMP_Text>().color = UserRecord.GetColorForUsername(manager.LocalUsername);
        }
    }

    private void OnDisable()
    {
        events.PushPinUpdated.RemoveListener(updatePushpinText);
        events.StarSelected.RemoveListener(starSelectedText);
        events.PlayerJoined.RemoveListener(playerListChanged);
        events.PlayerLeft.RemoveListener(playerListChanged);
    }

    private void OnDestroy()
    {
        events.PushPinUpdated.RemoveListener(updatePushpinText);
        events.StarSelected.RemoveListener(starSelectedText);
        events.PlayerJoined.RemoveListener(playerListChanged);
        events.PlayerLeft.RemoveListener(playerListChanged);
    }
    
}
