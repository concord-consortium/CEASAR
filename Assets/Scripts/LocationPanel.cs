using UnityEngine;
using TMPro;

public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;
    void Start ()
    {
        InteractionController.EarthIneractionDelegates += handleInteraction;
    }
 
    void OnDisable()
    {
        InteractionController.EarthIneractionDelegates += handleInteraction;
    }


    private void handleInteraction(Vector2 latLong)
    {
        if (latLongInfo)
        {
            latLongInfo.text = latLong.ToString();
        }
    }
}
