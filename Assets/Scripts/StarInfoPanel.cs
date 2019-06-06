using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StarInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI starInfoText;

    private bool isEnabled = true;

    public void setEnabled(bool enabled)
    {
        if (enabled)
        {
            isEnabled = true;
        }
        else
        {
            gameObject.SetActive(false);
            isEnabled = false;
        }
    }

    public void UpdateStarInfoPanel(Star starData)
    {
        if (isEnabled && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        string description = "";
        description += "Name: ";
        description += starData.ProperName.Length > 0 ? starData.ProperName : "N/A";
        description += "\n";
        description += "Hipparcos #: ";
        description += starData.Hipparcos.ToString();
        description += "\n";
        description += "Bayer Des.: ";
        description += starData.BayerDesignation.Length > 0 ? starData.BayerDesignation : "N/A";
        description += "\n";
        description += "Flamsteed Des.: ";
        description += starData.FlamsteedDesignation.Length > 0 ? starData.FlamsteedDesignation : "N/A";;
        description += "\n";
        description += "Constellation: ";
        description += starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A";
        description += "\n";
        description += "m: ";
        description += starData.Mag.ToString();
        description += "\n";
        starInfoText.text = description;
    }
}
