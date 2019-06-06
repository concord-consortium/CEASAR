using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

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
        StringBuilder description = new StringBuilder();
        description.Append("Name: ").AppendLine(starData.ProperName.Length > 0 ? starData.ProperName : "N/A");
        description.Append("Hipparcos #: ").AppendLine(starData.Hipparcos.ToString());
        description.Append("Bayer Des: ").AppendLine(starData.BayerDesignation.Length > 0 ? starData.BayerDesignation : "N/A");
        description.Append("Flamsteed Des: ").AppendLine(starData.FlamsteedDesignation.Length > 0 ? starData.FlamsteedDesignation : "N/A");
        description.Append("Constellation: ").AppendLine(starData.ConstellationFullName.Length > 0 ? starData.ConstellationFullName : "N/A");
        description.Append("m: ").AppendLine(starData.Mag.ToString());
        starInfoText.text = description.ToString();
    }
}
