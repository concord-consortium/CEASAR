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
    
    public TextMeshProUGUI pushPinDetailsText;
    public TextMeshProUGUI constellationText;
    public TextMeshProUGUI networkUsersText;

    private void OnEnable()
    {
        events.PushPinUpdated.AddListener(updatePushpinText);
        events.StarSelected.AddListener(starSelectedText);
    }

    private void updatePushpinText(Pushpin pin, Vector3 lookDirection)
    {
        pushPinDetailsText.text = pin.Location.ToDisplayString() + "\n" +
                                  pin.SelectedDateTime.ToShortDateString() + " " +
                                  pin.SelectedDateTime.ToShortTimeString();
    }
    
    private void starSelectedText(Star starData)
    {
        var longitudeTimeOffset = manager.CurrentLatLng.Longitude/15d;
        var lst = manager.CurrentSimulationTime.ToSiderealTime() + longitudeTimeOffset;
        AltAz altAz = Utils.CalculateAltitudeAzimuthForStar(starData.RA, starData.Dec,
            lst, manager.CurrentLatLng.Latitude);
        StringBuilder description = new StringBuilder();
        description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
        description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
        description.Append("Alt/Az: ").AppendLine(altAz.Altitude.ToString("F2") + ", " + altAz.Azimuth.ToString("F2"));
        description.Append("m: ").Append(starData.Mag.ToString());
        constellationText.text =  description.ToString();
    }
}
