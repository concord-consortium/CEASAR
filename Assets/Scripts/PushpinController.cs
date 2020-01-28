using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Here is where we control the placement of pushpins from remote users, and
/// show our own pushpin (which may move when in Horizon view?)
/// </summary>
public class PushpinController : MonoBehaviour
{
    
    public GameObject locationPinPrefab;

    private GameObject currentLocationPin;

    public GameObject CurrentLocationPin
    {
        get => currentLocationPin;
        set => currentLocationPin = value;
    }

    private List<GameObject> remotePins;
    private SimulationManager manager { get { return SimulationManager.GetInstance(); } }
    private SimulationEvents events { get { return SimulationEvents.GetInstance(); } }

    private void Start()
    {
        remotePins = new List<GameObject>();
    }

    public void AddPin(Vector3 pos, Quaternion rot, Color c, string pinOwner, LatLng latLng, DateTime pinDateTime, bool isLocal)
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
            currentLocationPin.transform.localRotation = rot;
            currentLocationPin.transform.position = pos;
            currentLocationPin.GetComponent<Renderer>().material.color = c;
            PushpinComponent pinObject = currentLocationPin.GetComponent<PushpinComponent>();
            pinObject.pin.Location = latLng;
            pinObject.pin.SelectedDateTime = manager.CurrentSimulationTime;
            
            // Update Simulation Manager with our pin
            manager.LocalUserPin = pinObject.pin;
            manager.Currentlocation = pinObject.pin.Location;
            manager.CurrentLocationName = SimulationConstants.CUSTOM_LOCATION;

            events.PushPinUpdated.Invoke(pos, rot, latLng, manager.CurrentSimulationTime);
            events.LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            
            string interactionInfo = "Pushpin set at: " + pinObject.pin.ToString(); // latLng.ToString() + " " + SimulationManager.GetInstance().CurrentSimulationTime;
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

            remotePin.transform.localRotation = rot;
            remotePin.transform.position = pos;
            remotePin.GetComponent<Renderer>().material.color = c;
            
            PushpinComponent pinObject = currentLocationPin.GetComponent<PushpinComponent>();
            pinObject.pin.Location = latLng;
            pinObject.pin.SelectedDateTime = pinDateTime;
        }
    }
   
    public void ShowPins(bool show)
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
        
    }
}
