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

    private GameObject currentLocationPin;
    private List<GameObject> remotePins;
    
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
        remotePins = new List<GameObject>();
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
            foreach (GameObject pin in remotePins)
            {
                pin.SetActive(show);
            }
        }

        if (currentLocationPin)
        {
            currentLocationPin.SetActive(show);
        } 
        
        if (show && manager.UserHasSetLocation)
        {
            UpdateLocalUserPin();
        }
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
                LatLng latLng = new LatLng{ Latitude = updatedPlayer.locationPin.latitude, Longitude = updatedPlayer.locationPin.longitude};
                AddOrUpdatePin(
                    latLng,
                    UserRecord.GetColorForUsername(updatedPlayer.username),
                    updatedPlayer.username, 
                    TimeConverter.JulianToCalendarDate(updatedPlayer.locationPin.datetime), 
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
        AddOrUpdatePin(latLng, playerColor, manager.LocalUsername, manager.CurrentSimulationTime, true, true);
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

    Vector3 getEarthRelativePos(LatLng latlng)
    {
        Vector3 earthRelativePos = new Vector3();
        if (earth)
        {
            Vector3 size = earth.GetComponent<Renderer>().bounds.size;
            float radius = size.x / 2;
            Vector3 pos = Utils.PositionFromLatLng(latlng, radius);
            earthRelativePos = pos - earth.transform.position; // Earth should be at 0,0,0 but in case it's moved, this would account for the difference
        }
        else
        {
            Debug.Log("No Earth found in interaction");
        }

        return earthRelativePos;
    }

    public void UpdateLocalUserPin()
    {
        string pinName = "pin_" + manager.LocalUsername;
        if (!currentLocationPin)
        {
            currentLocationPin = Instantiate(locationPinPrefab);
            currentLocationPin.name = pinName;
            currentLocationPin.transform.parent = this.transform;
        }

        Pushpin p = manager.LocalUserPin;
        Vector3 pos = getEarthRelativePos(p.Location);
        currentLocationPin.transform.localRotation = Quaternion.LookRotation(pos);
        currentLocationPin.transform.position = pos;
        currentLocationPin.GetComponent<Renderer>().material.color = manager.LocalPlayerColor;
        PushpinComponent pinObject = currentLocationPin.GetComponent<PushpinComponent>();
        pinObject.pin = p;
    }
    
    public void AddOrUpdatePin(LatLng latLng, Color c, string pinOwner, DateTime pinDateTime, bool isLocal,
        bool broadcast = false)
    {
        string pinName = "pin_" + pinOwner;
        if (isLocal)
        {
            if (!currentLocationPin)
            {
                currentLocationPin = Instantiate(locationPinPrefab);
                currentLocationPin.name = pinName;
                currentLocationPin.transform.parent = this.transform;
            }

            Vector3 pos = getEarthRelativePos(latLng);
            currentLocationPin.transform.localRotation = Quaternion.LookRotation(pos);
            currentLocationPin.transform.position = pos;
            currentLocationPin.GetComponent<Renderer>().material.color = c;
            PushpinComponent pinObject = currentLocationPin.GetComponent<PushpinComponent>();
            pinObject.pin.Location = latLng;
            pinObject.pin.SelectedDateTime = manager.CurrentSimulationTime;
            
            // Update Simulation Manager with our pin
            manager.LocalUserPin = pinObject.pin;
            manager.Currentlocation = pinObject.pin.Location;
            manager.CurrentLocationName = SimulationConstants.CUSTOM_LOCATION;

            events.PushPinUpdated.Invoke(latLng, manager.CurrentSimulationTime);
            if (broadcast)
            {
                // this can cause a feedback loop if we're merely moving the pin in response to a location change
                // so filter broadcasting this one so we only broadcast when we are adding a new pin in Earth scene
                events.LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            }

            string interactionInfo = "Pushpin set at: " + pinObject.pin.ToString(); 
            Debug.Log(interactionInfo);
            // CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
        else
        {
            GameObject remotePin = GameObject.Find(pinName);
            if (remotePin == null)
            {
                remotePin = Instantiate(locationPinPrefab);
                remotePin.name = pinName;
                remotePin.transform.parent = this.transform;
                remotePins.Add(remotePin);
            }
            Vector3 pos = getEarthRelativePos(latLng);
            remotePin.transform.localRotation = Quaternion.LookRotation(pos);;
            remotePin.transform.position = pos;
            remotePin.GetComponent<Renderer>().material.color = c;
                
            PushpinComponent pinObject = remotePin.GetComponent<PushpinComponent>();
            pinObject.pin.Location = latLng;
            pinObject.pin.SelectedDateTime = pinDateTime;
        }
        
    }
    void UpdatePinForLocalPlayer(LatLng latlng, string locationName)
    {
        // get 3d pos from latlng and update local player - this is in response to external location change
        // via dropdown
        AddOrUpdatePin(latlng, manager.LocalPlayer.color, manager.LocalPlayer.Username, manager.CurrentSimulationTime,
            true, false);
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