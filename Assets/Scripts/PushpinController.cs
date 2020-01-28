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
    private SimulationManager manager { get { return SimulationManager.GetInstance(); } }
    private SimulationEvents events { get { return SimulationEvents.GetInstance(); } }

    public void AddPin(Vector3 pos, Quaternion rot, Color c, string pinOwner, LatLng latLng, DateTime pinDateTime, bool isLocal)
    {
        if (isLocal)
        {
            if (!currentLocationPin) currentLocationPin = Instantiate(locationPinPrefab);
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

            events.PushPinUpdated.Invoke(latLng, manager.CurrentSimulationTime);
            events.LocationChanged.Invoke(latLng, SimulationConstants.CUSTOM_LOCATION);
            
            string interactionInfo = "Pushpin set at: " + pinObject.pin.ToString(); // latLng.ToString() + " " + SimulationManager.GetInstance().CurrentSimulationTime;
            Debug.Log(interactionInfo);
            // CCLogger.Log(CCLogger.EVENT_ADD_INTERACTION, interactionInfo);
        }
        else
        {
            string pinName = "pin_" + pinOwner;
            GameObject remotePin = GameObject.Find(pinName);
            if (remotePin == null)
            {
                remotePin = Instantiate(locationPinPrefab);
                remotePin.name = pinName;
            }

            remotePin.transform.localRotation = rot;
            remotePin.transform.position = pos;
            remotePin.GetComponent<Renderer>().material.color = c;
            PushpinComponent pinObject = currentLocationPin.GetComponent<PushpinComponent>();
            pinObject.pin.Location = latLng;
            pinObject.pin.SelectedDateTime = pinDateTime;
           
        }
    }
    // Likely more useful for remote player pins
    public void AddPin(Pushpin pin, Color c, string pinOwner, bool isLocal)
    {
        Vector3 newPos = new Vector3();
        this.AddPin(newPos, Quaternion.identity, c, pinOwner, pin.Location, pin.SelectedDateTime, isLocal);
        
    }
}
