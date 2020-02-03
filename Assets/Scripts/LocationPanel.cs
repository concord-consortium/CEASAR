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
    void Start()
    {
        if (SimulationManager.GetInstance().UserHasSetLocation)
        {
            UpdateLocationPanel(SimulationManager.GetInstance().Currentlocation, SimulationConstants.CUSTOM_LOCATION);
        }

        latLongInfo.enabled = SceneManager.GetActiveScene().name == SimulationConstants.SCENE_EARTH || showLocationCoordinates;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            showLocationCoordinates = !showLocationCoordinates;
            latLongInfo.enabled = showLocationCoordinates;
        }

    }
}
