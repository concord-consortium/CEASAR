﻿using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LocationSelectedEvent : UnityEvent<string> { }
[System.Serializable]
public class LocationChangeEvent : UnityEvent<LatLng, string> { }
[System.Serializable]
public class AnnotationAddedEvent : UnityEvent<Vector3, Quaternion, Vector3, string> { }
[System.Serializable]
public class AnnotationDeletedEvent : UnityEvent<string> { }
[System.Serializable]
public class AnnotationReceivedEvent : UnityEvent<NetworkTransform, Player> { }
[System.Serializable]
public class AnnotationClearEvent : UnityEvent<string> { }
[System.Serializable]
public class PushPinSelectedEvent : UnityEvent<LatLng, DateTime> { }
[System.Serializable]
public class PushPinUpdatedEvent : UnityEvent<Vector3, Quaternion, LatLng, DateTime> { }
[System.Serializable]
public class DrawModeEvent: UnityEvent<bool> { }
[System.Serializable]
public class PlayerJoinedEvent : UnityEvent<string> { }
public class PlayerLeftEvent : UnityEvent<string> { }
public class SimulationEvents
{
    protected SimulationEvents() 
    {
        if (LocationSelected == null) LocationSelected = new LocationSelectedEvent();
        if (LocationChanged == null) LocationChanged = new LocationChangeEvent();
        if (AnnotationAdded == null) AnnotationAdded = new AnnotationAddedEvent();
        if (AnnotationDeleted == null) AnnotationDeleted = new AnnotationDeletedEvent();
        if (AnnotationReceived == null) AnnotationReceived = new AnnotationReceivedEvent();
        if (AnnotationClear == null) AnnotationClear = new AnnotationClearEvent();
        if (PushPinSelected == null) PushPinSelected = new PushPinSelectedEvent();
        if (PushPinUpdated == null) PushPinUpdated = new PushPinUpdatedEvent();
        if (DrawMode == null) DrawMode = new DrawModeEvent();
        if (PlayerJoined == null) PlayerJoined = new PlayerJoinedEvent();
        if (PlayerLeft == null) PlayerLeft = new PlayerLeftEvent();
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
    /// This is where we handle specific annotations drawn in Horizon view
    /// </summary>
    public AnnotationAddedEvent AnnotationAdded;
    
    /// <summary>
    /// Deleting an annotation must be sync'd
    /// </summary>
    public AnnotationDeletedEvent AnnotationDeleted;
    
    /// <summary>
    /// Receiving annotations from remote players
    /// </summary>
    public AnnotationReceivedEvent AnnotationReceived;

    /// <summary>
    /// When a player leaves, clear their annotations
    /// </summary>
    public AnnotationClearEvent AnnotationClear;
    /// <summary>
    /// When a user wants to view the perspective of a pin
    /// </summary>
    public PushPinSelectedEvent PushPinSelected;
    /// <summary>
    /// When a user updates their own pushpin - this will network the event
    /// </summary>
    public PushPinUpdatedEvent PushPinUpdated;
    /// <summary>
    /// When a user enters / exits draw mode
    /// </summary>
    public DrawModeEvent DrawMode;
    /// <summary>
    /// When a user enters / exits draw mode
    /// </summary>
    public PlayerJoinedEvent PlayerJoined;
    /// <summary>
    /// When a user enters / exits draw mode
    /// </summary>
    public PlayerLeftEvent PlayerLeft;
}
