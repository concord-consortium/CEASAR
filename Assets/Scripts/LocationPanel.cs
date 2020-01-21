using UnityEngine;
using TMPro;

public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;
    
    void Start ()
    {
        SimulationEvents.GetInstance().LocationChanged.AddListener(updateLocationPanel);
        // InteractionController.EarthInteractionDelegates += handleInteraction;
    }
 
    void OnDisable()
    {
        SimulationEvents.GetInstance().LocationChanged.RemoveListener(updateLocationPanel);
        // InteractionController.EarthInteractionDelegates -= handleInteraction;
    }

    private void updateLocationPanel(Vector2 latLng, string description)
    {
        Debug.Log("update location panel");
        string newText = "";
        if (!string.IsNullOrEmpty(description))
        {
            newText += description + " ";
        }
        if (latLng != Vector2.zero)
        {
            newText += latLng.ToString();
        }
        if (latLongInfo)
        {
            latLongInfo.text = newText;
        }
        
    }

    private void handleInteraction(Vector2 latLong)
    {
        if (latLongInfo)
        {
            latLongInfo.text = latLong.ToString();
        }
    }
}
