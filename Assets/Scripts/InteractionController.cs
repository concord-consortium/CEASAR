using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine.SceneManagement;

public class InteractionController : MonoBehaviour
{
    public GameObject interactionIndicator;
    public GameObject locationPinPrefab;

    private GameObject localPlayerPinObject;
    //private List<GameObject> remotePins;
    private Dictionary<string, GameObject> remotePins;
    
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

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        remotePins = new Dictionary<string, GameObject>();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
        events.PushPinSelected.RemoveListener(UpdatePinForLocalPlayer);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;
        
        events.PushPinSelected.AddListener(UpdatePinForLocalPlayer);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        bool showPins = scene.name == "EarthInteraction";
        this.showPins(showPins);
    }
    
    private void OnSceneUnload(Scene scene)
    {
        // Forget about our cached earth object.
        _earth = null;
        // Don't need to forget about our network controller -- never disposed.
        // _networkController = null;
    }
    void showPins(bool show)
    {
        if (remotePins != null)
        {
            foreach (GameObject pin in remotePins.Values)
            {
                pin.SetActive(show);
            }
        }

        if (localPlayerPinObject)
        {
            localPlayerPinObject.SetActive(show);
        } 
        
        if (show && manager.UserHasSetLocation)
        {
            UpdateLocalUserPin();
        }
    }

    public void HandleRemoteInteraction(NetworkPlayer updatedNetworkPlayer, string interactionType)
    {
        switch (interactionType)
        {
            case "interaction":
                // show indicator
                Debug.Log("Interaction update: " + updatedNetworkPlayer.interactionTarget.position.x + "," +
                            updatedNetworkPlayer.interactionTarget.position.y + "," +
                            updatedNetworkPlayer.interactionTarget.position.z);
                ShowEarthMarkerInteraction(
                    Utils.NetworkV3ToVector3(updatedNetworkPlayer.interactionTarget.position), 
                    Utils.NetworkV3ToQuaternion(updatedNetworkPlayer.interactionTarget.rotation),
                    UserRecord.GetColorForUsername(updatedNetworkPlayer.username), false);
                break;
            case "celestialinteraction":
                Debug.Log("remote player selected star");
                // highlight star/ constellation
                // TODO: Adjust how we create stars to make it possible to find the star from the network interaction
                // this could be a simple rename, but need to check how constellation grouping works. Ideally we'll
                // maintain a dict of stars by ID for easier lookups. 
                StarComponent sc = manager.DataControllerComponent.GetStarById(updatedNetworkPlayer.celestialObjectTarget.uniqueId);
                sc.HandleSelectStar(false, UserRecord.GetColorForUsername(updatedNetworkPlayer.username));
                manager.GetRemotePlayer(updatedNetworkPlayer.username).SelectedCelestialSphereItem = sc.starData;
                break;
            case "locationpin":
                // add / move player pin
                Debug.Log("remote player pinned a location");
                Pushpin remotePlayerPin = NetworkPlayerPinToPushpin(updatedNetworkPlayer);
                Vector3 remotePlayerCameraRotation = NetworkPlayerCameraRotation(updatedNetworkPlayer);
               
                Player remotePlayer = manager.GetRemotePlayer(updatedNetworkPlayer.username);
                remotePlayer.UpdatePlayerLookDirection(remotePlayerCameraRotation);
                remotePlayer.Pin = remotePlayerPin;
                AddOrUpdatePin(
                    remotePlayer.Pin,
                    UserRecord.GetColorForUsername(updatedNetworkPlayer.username),
                    updatedNetworkPlayer.username, 
                    false); 
                break;
            case "annotation":
                // add annotation
                ArraySchema<NetworkTransform> annotations = updatedNetworkPlayer.annotations;
                NetworkTransform lastAnnotation = annotations[annotations.Count - 1];
                
                events.AnnotationReceived.Invoke(
                    lastAnnotation,
                    updatedNetworkPlayer);
                break;
            default:
                break;
        }
        //}
    }

    public Pushpin NetworkPlayerPinToPushpin(NetworkPlayer networkPlayer)
    {
        LatLng latLng = new LatLng{ Latitude = networkPlayer.locationPin.latitude, Longitude = networkPlayer.locationPin.longitude};
        DateTime dt = DateTime.UtcNow;
        dt = TimeConverter.EpochTimeToDate(networkPlayer.locationPin.datetime);
        return new Pushpin(TimeConverter.EpochTimeToDate(networkPlayer.locationPin.datetime), latLng,
            networkPlayer.locationPin.locationName);
    }

    public Vector3 NetworkPlayerCameraRotation(NetworkPlayer networkPlayer)
    {
        Quaternion remotePlayerCameraRotationRaw =
            Utils.NetworkV3ToQuaternion(networkPlayer.locationPin.cameraTransform.rotation);
        return remotePlayerCameraRotationRaw.eulerAngles;
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
            // SimulationEvents.GetInstance().LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
    }
    public void SetEarthLocationPin(Vector3 pos, Quaternion rot)
    {
        LatLng latLng = getEarthRelativeLatLng(pos);
        Pushpin p = new Pushpin(manager.CurrentSimulationTime, latLng, SimulationConstants.CUSTOM_LOCATION);
        AddOrUpdatePin(p, manager.LocalPlayerColor, manager.LocalUsername, true, true);
    }

    string getPinName(string pinOwner)
    {
        return SimulationConstants.PIN_PREFIX + pinOwner;
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
        return latLng;
    }

    Vector3 getEarthRelativePos(LatLng latlng)
    {
        Vector3 earthRelativePos = new Vector3();
        if (earth)
        {
            Vector3 size = earth.GetComponent<Renderer>().bounds.size;
            float radius = (size.x / 2) - 0.1f;
            Vector3 pos = Utils.PositionFromLatLng(latlng, radius);
            earthRelativePos = pos - earth.transform.position; // Earth should be at 0,0,0 but in case it's moved, this would account for the difference
        }
        else
        {
            Debug.Log("No Earth found in interaction");
        }

        return earthRelativePos;
    }

    /// <summary>
    /// When the scene changes, refresh the pin location if it was changed in Horizon view
    /// </summary>
    public void UpdateLocalUserPin()
    {
        if (manager.UserHasSetLocation && manager.LocalUserPin != null)
        {
            string pinName = getPinName(manager.LocalUsername);
            if (localPlayerPinObject == null)
            {
                localPlayerPinObject = getPinObject(pinName);
            }

            Pushpin p = manager.LocalUserPin;
            updatePinObject(localPlayerPinObject, manager.LocalUserPin, manager.LocalPlayerColor);
        }
    }
    
    // This is used for local pins and remote pins
    public void AddOrUpdatePin(Pushpin pin, Color c, string pinOwner, bool isLocal, bool broadcast = false)
    {
        if (isLocal)
        {
            if (localPlayerPinObject == null)
            {
                localPlayerPinObject = Instantiate(locationPinPrefab);
                localPlayerPinObject.name = getPinName(pinOwner);
                localPlayerPinObject.transform.parent = this.transform;
                
            }
            updatePinObject(localPlayerPinObject, pin, manager.LocalPlayerColor);

            // Update Simulation Manager with our pin
            // manager.LocalUserPin = p;
            // manager.CurrentLatLng = p.Location;
            // manager.CurrentLocationName = SimulationConstants.CUSTOM_LOCATION;
            
            events.PushPinUpdated.Invoke(manager.LocalUserPin, manager.LocalPlayerLookDirection);
            if (broadcast)
            {
                // this can cause a feedback loop if we're merely moving the pin in response to a location change
                // so filter broadcasting this one so we only broadcast when we are adding a new pin in Earth scene
                events.PushPinSelected.Invoke(pin);
            }
            //
            // string interactionInfo = "Pushpin set at: " + p.ToString(); 
            // Debug.Log(interactionInfo);
            // CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
        else
        {
            if (remotePins[pinOwner] == null)
            {
                GameObject pinObject = Instantiate(locationPinPrefab);
                pinObject.name = getPinName(pinOwner);
                pinObject.transform.parent = this.transform;
                remotePins[pinOwner] = pinObject;
            }
            // update the 3d object in scene with correct location
            updatePinObject(remotePins[pinOwner], pin, c);
        }
        
    }
    void UpdatePinForLocalPlayer(Pushpin pin)
    {
        // this is in response to external location change via dropdown - we update, but do not broadcast
        AddOrUpdatePin(pin, manager.LocalPlayerRecord.color, manager.LocalPlayerRecord.Username, true, false);
    }

    GameObject getPinObject(string pinName)
    {
        GameObject pinObject = GameObject.Find(pinName);
        if (pinObject == null)
        {
            pinObject = Instantiate(locationPinPrefab);
            pinObject.name = pinName;
            pinObject.transform.parent = this.transform;
        }

        return pinObject;
    }
    void updatePinObject(GameObject pinObject, Pushpin pin, Color c)
    {
        Vector3 pos = getEarthRelativePos(pin.Location);
        pinObject.transform.localRotation = pos == Vector3.zero ? Quaternion.Euler(Vector3.zero) : Quaternion.LookRotation(pos);
        pinObject.transform.position = pos;
        pinObject.GetComponent<Renderer>().material.color = c;
        PushpinComponent pinComponent = pinObject.GetComponent<PushpinComponent>();
        pinComponent.UpdatePin(pin);
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