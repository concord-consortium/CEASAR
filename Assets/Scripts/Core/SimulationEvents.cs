using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LocationSelectedEvent : UnityEvent<string> { }
// [System.Serializable]
// public class LocationChangeEvent : UnityEvent<LatLng, string> { }
// [System.Serializable]
public class AnnotationAddedEvent : UnityEvent<Vector3, Quaternion, Vector3, string> { }
[System.Serializable]
public class AnnotationDeletedEvent : UnityEvent<string> { }
[System.Serializable]
public class AnnotationReceivedEvent : UnityEvent<NetworkTransform, NetworkPlayer> { }
[System.Serializable]
public class AnnotationClearEvent : UnityEvent<string> { }
[System.Serializable]
public class PushPinSelectedEvent : UnityEvent<Pushpin> { }
[System.Serializable]
public class PushPinUpdatedEvent : UnityEvent<Pushpin, Vector3> { }
[System.Serializable]
public class PlayerNorthPinEvent: UnityEvent<float> { }
[System.Serializable]
public class DrawModeEvent: UnityEvent<bool> { }
[System.Serializable]
public class PlayerJoinedEvent : UnityEvent<string> { }
[System.Serializable]
public class PlayerLeftEvent : UnityEvent<string> { }
[System.Serializable]
public class SimulationTimeChangedEvent : UnityEvent { }
public class SnapshotCreatedEvent: UnityEvent<Pushpin> { }
public class SnapshotDeletedEvent: UnityEvent<Pushpin> { }
public class SnapshotLoadedEvent: UnityEvent<Pushpin> { }
public class NetworkConnectionEvent: UnityEvent<bool> { }
public class NetworkConnectionUpdateEvent: UnityEvent<bool> { }
[System.Serializable] public class StarSelectedEvent: UnityEvent<Star> { }
[System.Serializable] public class ConstellationSelectedEvent: UnityEvent<string> { }
public class SimulationEvents
{
    protected SimulationEvents() 
    {
        if (LocationSelected == null) LocationSelected = new LocationSelectedEvent();
        if (AnnotationAdded == null) AnnotationAdded = new AnnotationAddedEvent();
        if (AnnotationDeleted == null) AnnotationDeleted = new AnnotationDeletedEvent();
        if (AnnotationReceived == null) AnnotationReceived = new AnnotationReceivedEvent();
        if (AnnotationClear == null) AnnotationClear = new AnnotationClearEvent();
        if (PushPinSelected == null) PushPinSelected = new PushPinSelectedEvent();
        if (PushPinUpdated == null) PushPinUpdated = new PushPinUpdatedEvent();
        if (PlayerNorthPin == null) PlayerNorthPin = new PlayerNorthPinEvent();
        if (DrawMode == null) DrawMode = new DrawModeEvent();
        if (PlayerJoined == null) PlayerJoined = new PlayerJoinedEvent();
        if (PlayerLeft == null) PlayerLeft = new PlayerLeftEvent();
        if (SimulationTimeChanged == null) SimulationTimeChanged = new SimulationTimeChangedEvent();
        if (SnapshotCreated == null) SnapshotCreated = new SnapshotCreatedEvent();
        if (SnapshotLoaded == null) SnapshotLoaded = new SnapshotLoadedEvent();
        if (SnapshotDeleted == null) SnapshotDeleted = new SnapshotDeletedEvent();
        if (NetworkConnection == null) NetworkConnection = new NetworkConnectionEvent();
        if (NetworkUpdate == null) NetworkUpdate = new NetworkConnectionUpdateEvent();
        if (StarSelected == null) StarSelected = new StarSelectedEvent();
        if (ConstellationSelected == null) ConstellationSelected = new ConstellationSelectedEvent();
    }
    private static SimulationEvents instance;

    public static SimulationEvents Instance
    {
        get { return instance ?? (instance = new SimulationEvents()); }
    }

    /// <summary>
    /// Use this event to listen to selection of a new location (not the actual change event)
    /// This is more useful when selecting the location from the drop down panel.
    /// </summary>
    public LocationSelectedEvent LocationSelected;

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
    ///  When a user places the North pin in the direction they believe is North
    /// </summary>
    public PlayerNorthPinEvent PlayerNorthPin;
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

    /// <summary>
    /// When the simulation time changes, the user's pushpin needs to be updated
    /// </summary>
    public SimulationTimeChangedEvent SimulationTimeChanged;

    /// <summary>
    /// For logging the creation of a snapshot
    /// </summary>
    public SnapshotCreatedEvent SnapshotCreated;
    /// <summary>
    /// For logging the selection of a snapshot
    /// </summary>
    public SnapshotLoadedEvent SnapshotLoaded;
    /// <summary>
    /// For logging the deletion of a snapshot
    /// </summary>
    public SnapshotDeletedEvent SnapshotDeleted;

    /// <summary>
    /// For monitoring changes in network connection status via Colyseus
    /// </summary>
    public NetworkConnectionEvent NetworkConnection;
    /// <summary>
    /// For updating changes in network connection status via UI
    /// </summary>
    public NetworkConnectionUpdateEvent NetworkUpdate;
    /// <summary>
    /// For when users select a star
    /// </summary>
    public StarSelectedEvent StarSelected;

    public ConstellationSelectedEvent ConstellationSelected;
}
