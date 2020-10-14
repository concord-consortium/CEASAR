using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using UnityEngine.SceneManagement;
using static SimulationConstants;

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

    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

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
        if (remotePins != null && remotePins.Count > 0)
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
        
        if (show)
        {
            UpdateLocalUserPinObject();
        }
    }

    public void HandleRemoteInteraction(NetworkPlayer updatedNetworkPlayer, NetworkMessageType interactionType)
    {
        switch (interactionType)
        {
            case NetworkMessageType.Interaction:
                // show indicator
                CCDebug.Log("Interaction update: " + updatedNetworkPlayer.interactionTarget.position.x + "," +
                            updatedNetworkPlayer.interactionTarget.position.y + "," +
                            updatedNetworkPlayer.interactionTarget.position.z, LogLevel.Info, LogMessageCategory.Networking);
                ShowEarthMarkerInteraction(
                    Utils.NetworkV3ToVector3(updatedNetworkPlayer.interactionTarget.position), 
                    Utils.NetworkV3ToQuaternion(updatedNetworkPlayer.interactionTarget.rotation),
                    UserRecord.GetColorForUsername(updatedNetworkPlayer.username), false);
                break;
            case NetworkMessageType.CelestialInteraction:
                CCDebug.Log("remote player selected star", LogLevel.Info, LogMessageCategory.Networking);
                // highlight star/ constellation
                // TODO: Adjust how we create stars to make it possible to find the star from the network interaction
                // this could be a simple rename, but need to check how constellation grouping works. Ideally we'll
                // maintain a dict of stars by ID for easier lookups. 
                StarComponent sc = manager.DataControllerComponent.GetStarById(updatedNetworkPlayer.celestialObjectTarget.uniqueId);
                sc.HandleSelectStar(false, UserRecord.GetColorForUsername(updatedNetworkPlayer.username));
                manager.GetRemotePlayer(updatedNetworkPlayer.username).SelectedCelestialSphereItem = sc.starData;
                break;
            case NetworkMessageType.LocationPin:
                // add / move player pin
                CCDebug.Log("remote player pinned a location", LogLevel.Info, LogMessageCategory.Networking);
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
            case NetworkMessageType.Annotation:
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
            CCLogger.Log(LOG_EVENT_INTERACTION_ADDED, interactionInfo);
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
            // SimulationEvents.Instance.LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            CCLogger.Log(LOG_EVENT_INTERACTION_ADDED, interactionInfo);
        }
    }
    /// <summary>
    /// Set a new location pin from Earth scene by detecting the latlng from 3d position
    /// </summary>
    /// <param name="pos">Position on the surface of the Earth that was clicked</param>
    public void SetEarthLocationPin(Vector3 pos)
    {
        LatLng latLng = getEarthRelativeLatLng(pos);
        Pushpin p = new Pushpin(manager.CurrentSimulationTime, latLng, SimulationConstants.CUSTOM_LOCATION);
        manager.JumpToPin(p);
        // broadcast the update
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
        events.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
        AddOrUpdatePin(p, manager.LocalPlayerColor, manager.LocalUsername, true);
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
        return earthRelativePos;
    }

    /// <summary>
    /// When the scene changes, refresh the pin location if it was changed in Horizon view
    /// </summary>
    public void UpdateLocalUserPinObject()
    {
        string pinName = getPinName(manager.LocalUsername);
        if (localPlayerPinObject == null) 
        {
            localPlayerPinObject = getPinObject(pinName);
            localPlayerPinObject.GetComponent<PushpinComponent>().owner = manager.LocalUsername;
        }

        updatePinObject(localPlayerPinObject, manager.LocalPlayerPin, manager.LocalPlayerColor);
    }
    
    // This is used for local pins and remote pins
    public void AddOrUpdatePin(Pushpin pin, Color c, string pinOwner, bool isLocal)
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
            // events.PushPinSelected.Invoke(pin);
        }
        else
        {
            if (!remotePins.ContainsKey(pinOwner))
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
        AddOrUpdatePin(pin, manager.LocalPlayerRecord.color, manager.LocalPlayerRecord.Username, true);
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
    // Update the visible pin in-game to show at the correct location with the correct color.
    void updatePinObject(GameObject pinObject, Pushpin pin, Color c)
    {
        Vector3 pos = getEarthRelativePos(pin.Location);
        pinObject.transform.localRotation = pos == Vector3.zero ? Quaternion.Euler(Vector3.zero) : Quaternion.LookRotation(pos);
        pinObject.transform.position = pos;
        pinObject.GetComponent<Renderer>().material.color = c;
        
        // HIDE IF WE ARE AT THE CRASH SITE
        if (pin.IsCrashSite())
        {
            pinObject.SetActive(false);
        }
        else
        {
            pinObject.SetActive(true);
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