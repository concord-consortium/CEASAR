using UnityEngine;
using TMPro;

public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;
    void Start ()
    {
        InteractionController.EarthInteractionDelegates += handleInteraction;
    }
 
    void OnDisable()
    {
        InteractionController.EarthInteractionDelegates -= handleInteraction;
    }


    private void handleInteraction(Vector2 latLong)
    {
        if (latLongInfo)
        {
            latLongInfo.text = latLong.ToString();
        }
    }
}
