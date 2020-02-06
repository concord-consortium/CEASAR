using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// This panel displays the currently-selected location in a user friendly manner
/// </summary>
public class LocationPanel : MonoBehaviour
{
    public bool showLocationCoordinates = false;
    public bool showCompass = false;
    public TextMeshProUGUI latLongInfo;
    
    private SimulationManager manager { get { return SimulationManager.GetInstance();}}
    void Start()
    {
        
        UpdateLocationPanel(manager.CurrentLatLng, manager.CurrentLocationName);

        latLongInfo.enabled = SceneManager.GetActiveScene().name == SimulationConstants.SCENE_EARTH || showLocationCoordinates;
    }
    public void UpdateLocationPanel(LatLng latLng, string description)
    {
        // HIDE IF WE ARE AT CRASH SITE!
        if (latLng == manager.CrashSiteForGroup.Location)
        {
            latLongInfo.text = "At the crash site";
        }
        else
        {
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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            showLocationCoordinates = !showLocationCoordinates;
            latLongInfo.enabled = showLocationCoordinates;
        }

    }
}
