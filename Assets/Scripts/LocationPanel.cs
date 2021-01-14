using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// This panel displays the currently-selected location in a user friendly manner
/// </summary>
public class LocationPanel : MonoBehaviour
{
    public TextMeshProUGUI latLongInfo;
    
    private SimulationManager manager { get { return SimulationManager.Instance;}}
    void Start()
    {
        UpdateLocationPanel(manager.LocalPlayerPin);
    }
    public void UpdateLocationPanel(Pushpin pin)
    {
        if (!latLongInfo) latLongInfo = GetComponent<TextMeshProUGUI>();
        // HIDE IF WE ARE AT CRASH SITE! The Pushpin itself hides coordinates for all crash sites that are a match,
        // but for our current crash site (by group) we display custom text.
        if (pin.Location == manager.CrashSiteForGroup.Location)
        {
            latLongInfo.text = "At the crash site";
        }
        else
        {
            latLongInfo.text = pin.LocationNameDetail;
        }
    }

    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.L))
    //     {
    //         showLocationCoordinates = !showLocationCoordinates;
    //         latLongInfo.enabled = showLocationCoordinates;
    //     }
    //
    // }
}
