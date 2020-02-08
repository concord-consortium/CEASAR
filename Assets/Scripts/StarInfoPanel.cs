using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class StarInfoPanel : MonoBehaviour
{
    private SimulationManager manager;
    public TextMeshProUGUI starInfoText;

    public void Setup()
    {
        manager = SimulationManager.GetInstance();
        if (manager.CurrentlySelectedStar == null)
        {
            starInfoText.text = "";
        }
        else if (string.IsNullOrEmpty(starInfoText.text))
        {
            UpdateStarInfoPanel();
            CCConsoleLog.Log("Highlighting constellation " + manager.CurrentlySelectedStar.starData.ConstellationFullName);
            ConstellationsController constellationsController = FindObjectOfType<ConstellationsController>();
            if (constellationsController)
            {
                constellationsController.HighlightSingleConstellation(manager.CurrentlySelectedStar.starData.ConstellationFullName);
            }
        }
    }

    public void UpdateStarInfoPanel()
    {
        manager = SimulationManager.GetInstance();
        if (manager.CurrentlySelectedStar != null)
        {
            Star starData = manager.CurrentlySelectedStar.starData;
            AltAz altAz = Utils.CalculateAltitudeAzimuthForStar(starData.RA, starData.Dec,
                manager.CurrentSimulationTime.ToSiderealTime(), manager.CurrentLatLng.Latitude);
            StringBuilder description = new StringBuilder();
            description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
            description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
            description.Append("Altitude: ").AppendLine(altAz.Altitude.ToString());
            description.Append("Azimuth: ").AppendLine(altAz.Azimuth.ToString());
            // description.Append("Hipparcos #: ").AppendLine(starData.Hipparcos.ToString());
            // description.Append("Bayer Des: ").AppendLine(starData.BayerDesignation.Length > 0 ? starData.BayerDesignation : "N/A");
            // description.Append("Flamsteed Des: ").AppendLine(starData.FlamsteedDesignation.Length > 0 ? starData.FlamsteedDesignation : "N/A");
            description.Append("m: ").AppendLine(starData.Mag.ToString());
            starInfoText.text = description.ToString();
        } 
        else 
        {
            starInfoText.text = "";
        }
    }
}
