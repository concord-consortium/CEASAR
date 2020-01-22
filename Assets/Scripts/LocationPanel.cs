using UnityEngine;
using TMPro;

public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;

    public void UpdateLocationPanel(Vector2 latLng, string description)
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
