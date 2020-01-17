using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class InteractionController : MonoBehaviour
{
    public delegate void OnEarthInteraction(Vector2 LatLong);
    public static OnEarthInteraction EarthIneractionDelegates;

    public GameObject interactionIndicator;

    // Cached reference to earth object for lat/lng
    private GameObject _earth;
    private GameObject earth
    {
        get
        {
            if(_earth == null)
            {
                // Look up and cache the local earth object
                _earth = GameObject.FindGameObjectWithTag("Earth");
            }
            return _earth;
        }
    }

    // Cached reference to Network controller for broadcasting events
    private NetworkController _networkController;
    private NetworkController networkController
    {
        get {
            if (_networkController == null)
            {
                // Look up and cache our network controller
                _networkController = FindObjectOfType<NetworkController>();
            }
            return _networkController;
        }
    }

    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnload;
    }


    private void OnSceneUnload(Scene scene)
    {
        // Forget about our cached earth object.
        _earth = null;
        // Don't need to forget about our network controller -- never disposed.
        // _networkController = null;
    }

    public void HandleRemoteInteraction(Player updatedPlayer, string interactionType)
    {
        switch (interactionType)
        {
            case "interaction":
                // show indicator
                Debug.Log("Interaction update: " + updatedPlayer.interactionTarget.position.x + "," +
                            updatedPlayer.interactionTarget.position.y + "," +
                            updatedPlayer.interactionTarget.position.z);
                Vector3 pos = Utils.NetworkPosToPosition(updatedPlayer.interactionTarget.position);
                Quaternion rot = Utils.NetworkRotToRotation(updatedPlayer.interactionTarget.rotation);
                ShowEarthMarkerInteraction(pos, rot,
                    SimulationManager.GetInstance().GetColorForUsername(updatedPlayer.username), false);
                break;
            case "celestialinteraction":
                Debug.Log("remote player selected star");
                // highlight star/ constellation
                // TODO: Adjust how we create stars to make it possible to find the star from the network interaction
                // this could be a simple rename, but need to check how constellation grouping works. Ideally we'll
                // maintain a dict of stars by ID for easier lookups. 
                SimulationManager.GetInstance().DataControllerComponent.GetStarById(updatedPlayer.celestialObjectTarget.uniqueId).HandleSelectStar();
                break;
            case "locationpin":
                // add / move player pin
                Debug.Log("remote player pinned a location");
                break;
            default:
                break;
        }
        //}
    }

    public void ShowCelestialObjectInteraction(string coName, string coGroup, string uniqueId, bool isLocal)
    {
        if (isLocal)
        {
            // now need to broadcast to remotes
            NetworkCelestialObject co = new NetworkCelestialObject
            {
                name = coName,
                group = coGroup,
                uniqueId = uniqueId
            };
            if (networkController)
            {
                networkController.BroadcastCelestialInteraction(co);
            }
            string interactionInfo = "local celestial interaction CO:" +
                                     coName + ", " + coGroup + ", " + uniqueId;
            CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
    }

    public void ShowEarthMarkerInteraction(Vector3 pos, Quaternion rot, Color playerColor, bool isLocal)
    {
        Vector2 latLng = Vector2.zero; // unset value
        if (earth)
        {
            Vector3 earthPos = pos - earth.transform.position;
            Vector3 size = earth.GetComponent<Renderer>().bounds.size;
            float radius = size.x / 2;
            latLng = Utils.LatLngFromPosition(earthPos, radius);
        }
        else
        {
            Debug.Log("No Earth found in interacion, using 0 for lat & lng");
        }

        if (interactionIndicator)
        {
            GameObject indicatorObj = Instantiate(interactionIndicator);
            indicatorObj.transform.localRotation = rot;
            indicatorObj.transform.position = pos;
            Utils.SetObjectColor(indicatorObj, playerColor);
            StartCoroutine(selfDestruct(indicatorObj));
        }
        if (isLocal)
        {
            if(networkController)
            {
                networkController.BroadcastEarthInteraction(pos, rot);
            }
            string interactionInfo = "Earth interaction at: " + latLng.ToString();
            Debug.Log(interactionInfo);
            EarthIneractionDelegates(latLng);
            CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
    }

    IEnumerator selfDestruct(GameObject indicatorObj)
    {
        yield return new WaitForSeconds(3.0f);
        if (indicatorObj)
        {
            Destroy(indicatorObj);
        }
    }

}