using System.Linq;
using UnityEngine;
using TMPro;

public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;

    void Start()
    {
        if (SimulationManager.GetInstance().UserHasSetLocation)
        {
            UpdateLocationPanel(SimulationManager.GetInstance().Currentlocation, SimulationConstants.CUSTOM_LOCATION);
        }
    }
    public void UpdateLocationPanel(LatLng latLng, string description)
    {
        Debug.Log("update location panel");
        string newText = "";
        if (!string.IsNullOrEmpty(description))
        {
            newText += description + " ";
        }
        newText += latLng.ToDisplayString();
        
        if (latLongInfo)
        {
            latLongInfo.text = newText;
        }
    }

    private void handleInteraction(LatLng latLong)
    {
        if (latLongInfo)
        {
            latLongInfo.text = latLong.ToString();
        }
    }
}
