using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LocationSelectedEvent : UnityEvent<string> { }
[System.Serializable]
public class LocationChangeEvent : UnityEvent<Vector2, string> { }
[System.Serializable]
public class AnnotationAddedEvent : UnityEvent<Vector3, Vector3> { }
[System.Serializable]
public class PushPinCreatedEvent : UnityEvent<Vector2, DateTime> { }
[System.Serializable]
public class PushPinUpdatedEvent : UnityEvent<Vector2, DateTime> { }
public class SimulationEvents
{
    protected SimulationEvents() 
    {
        if (LocationSelected == null) LocationSelected = new LocationSelectedEvent();
        if (LocationChanged == null) LocationChanged = new LocationChangeEvent();
        if (AnnotationAdded == null) AnnotationAdded = new AnnotationAddedEvent();
        if (PushPinCreated == null) PushPinCreated = new PushPinCreatedEvent();
        if (PushPinUpdated == null) PushPinUpdated = new PushPinUpdatedEvent();
    }
    private static SimulationEvents instance;

    public static SimulationEvents GetInstance()
    {
        return instance ?? (instance = new SimulationEvents());
    }

    /// <summary>
    /// Use this event to listen to selection of a new location (not the actual change event)
    /// This is more useful when selecting the location from the drop down panel.
    /// </summary>
    public LocationSelectedEvent LocationSelected;


    /// <summary>
    /// Use this to listen for changes to location
    /// </summary>
    public LocationChangeEvent LocationChanged;

    /// <summary>
    /// This is where we handle specific locations, set from Earth scene
    /// </summary>
    public AnnotationAddedEvent AnnotationAdded;

    /// <summary>
    /// When a user first creates a pushpin
    /// </summary>
    public PushPinCreatedEvent PushPinCreated;
    /// <summary>
    /// When a user updates a pushpin
    /// </summary>
    public PushPinUpdatedEvent PushPinUpdated;
}
