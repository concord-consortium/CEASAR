using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SimulationConstants
{
    public static readonly string CUSTOM_LOCATION = "(Custom Location)";
    public static readonly string PIN_PREFIX = "pin_";
    
    // Player prefs keys
    public static readonly string SNAPSHOT_LOCATION_PREF_KEY = "SnapshotLocation";
    public static readonly string SNAPSHOT_DATETIME_PREF_KEY = "SnapshotDateTime";
    public static readonly string SNAPSHOT_LOCATION_COORDS_PREF_KEY = "SnapshotLocationCoords";

    // Scene Names:
    public static readonly string SCENE_LOAD = "LoadSim";
    public static readonly string SCENE_STARS = "Stars";
    public static readonly string SCENE_HORIZON = "Horizon";
    public static readonly string SCENE_EARTH = "EarthInteraction";

    // Platform Scene Listings:
    public static readonly string[] SCENES_VR = { SCENE_LOAD, SCENE_STARS, SCENE_HORIZON, SCENE_EARTH };
    public static readonly string[] SCENES_AR = { SCENE_LOAD, SCENE_STARS, SCENE_HORIZON, SCENE_EARTH };
    public static readonly string[] SCENES_PC = { SCENE_LOAD, SCENE_STARS, SCENE_HORIZON, SCENE_EARTH };
    public static readonly string[] SCENES_PLAYABLE = { SCENE_STARS, SCENE_HORIZON, SCENE_EARTH };
}
