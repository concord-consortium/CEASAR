using System;
using UnityEngine;
using System.Collections;
using Colyseus.Schema;
using UnityEngine.SceneManagement;

public class InteractionController : MonoBehaviour
{
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

    private SimulationManager manager { get { return SimulationManager.GetInstance(); } }
    private SimulationEvents events { get { return SimulationEvents.GetInstance(); } }

    private PushpinController _pushpinController;
    private PushpinController pushpinController
    {
        get
        {
            if (_pushpinController == null) _pushpinController = this.GetComponent<PushpinController>();
            return _pushpinController;
        }
    }
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
        events.LocationChanged.RemoveListener(UpdatePinForLocalPlayer);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;
        
        events.LocationChanged.AddListener(UpdatePinForLocalPlayer);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        bool showPins = scene.name == "EarthInteraction";
        pushpinController.ShowPins(showPins);
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
               
                ShowEarthMarkerInteraction(
                    Utils.NetworkV3ToVector3(updatedPlayer.interactionTarget.position), 
                    Utils.NetworkV3ToQuaternion(updatedPlayer.interactionTarget.rotation),
                    UserRecord.GetColorForUsername(updatedPlayer.username), false);
                break;
            case "celestialinteraction":
                Debug.Log("remote player selected star");
                // highlight star/ constellation
                // TODO: Adjust how we create stars to make it possible to find the star from the network interaction
                // this could be a simple rename, but need to check how constellation grouping works. Ideally we'll
                // maintain a dict of stars by ID for easier lookups. 
                manager.DataControllerComponent.GetStarById(updatedPlayer.celestialObjectTarget.uniqueId).HandleSelectStar();
                break;
            case "locationpin":
                // add / move player pin
                Debug.Log("remote player pinned a location");
                Vector3 pos = Utils.NetworkV3ToVector3(updatedPlayer.locationPin.location.position);
                LatLng latLng = getEarthRelativeLatLng(pos);
                pushpinController.AddPin(
                    pos,
                    Utils.NetworkV3ToQuaternion(updatedPlayer.locationPin.location.rotation),
                    UserRecord.GetColorForUsername(updatedPlayer.username),
                    updatedPlayer.username, 
                    latLng,
                    new DateTime((long)updatedPlayer.locationPin.datetime), 
                    false); 
                break;
            case "annotation":
                // add annotation
                ArraySchema<NetworkTransform> annotations = updatedPlayer.annotations;
                NetworkTransform lastAnnotation = annotations[annotations.Count - 1];
                
                events.AnnotationReceived.Invoke(
                    lastAnnotation,
                    updatedPlayer);
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
        LatLng latLng = getEarthRelativeLatLng(pos);

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
            SimulationEvents.GetInstance().LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
    }
    public void SetEarthLocationPin(Vector3 pos, Quaternion rot, Color playerColor, bool isLocal)
    {
        LatLng latLng = getEarthRelativeLatLng(pos);

        pushpinController.AddPin(pos, rot, playerColor, manager.LocalUsername, latLng, manager.CurrentSimulationTime, true);
    }

    LatLng getEarthRelativeLatLng(Vector3 pos)
    {
        LatLng latLng = new LatLng(); // unset value
        if (earth)
        {
            Vector3 earthPos = pos - earth.transform.position; // Earth should be at 0,0,0 but in case it's moved, this would account for the difference
            Vector3 size = earth.GetComponent<Renderer>().bounds.size;
            float radius = size.x / 2;
            latLng = Utils.LatLngFromPosition(earthPos, radius);
        }
        else
        {
            Debug.Log("No Earth found in interaction, using 0 for lat & lng");
        }

        return latLng;
    }
    
    void UpdatePinForLocalPlayer(LatLng latlng, string locationName)
    {
        // get 3d pos from latlng
        if (earth)
        {
            Vector3 size = earth.GetComponent<Renderer>().bounds.size;
            float radius = size.x / 2;
            Vector3 pos = Utils.PositionFromLatLng(latlng, radius);
            Vector3 earthRelativePos = pos - earth.transform.position; // Earth should be at 0,0,0 but in case it's moved, this would account for the difference

            pushpinController.CurrentLocationPin.transform.position = earthRelativePos;
            pushpinController.CurrentLocationPin.transform.rotation = Quaternion.LookRotation(earthRelativePos);
        }
        else
        {
            Debug.Log("No Earth found in interaction");
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