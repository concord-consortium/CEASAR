public static class SimulationConstants
{
    public static readonly string CUSTOM_LOCATION = "(Custom Location)";
    public static readonly string PIN_PREFIX = "pin_";
    
    // Player prefs keys
    public static readonly string SNAPSHOT_LOCATION_PREF_KEY = "SnapshotLocation";
    public static readonly string SNAPSHOT_DATETIME_PREF_KEY = "SnapshotDateTime";
    public static readonly string SNAPSHOT_LOCATION_COORDS_PREF_KEY = "SnapshotLocationCoords";
    public static readonly string USERNAME_PREF_KEY = "CEASAR_USERNAME";
    public static readonly string USER_GROUP_PREF_KEY = "CEASAR_GROUP";
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
    
    // UI Panel Names:
    public static readonly string PANEL_CONSTELLATION = "ConstellationSelectionPanel";
    public static readonly string PANEL_CURRENTTIME = "CurrentTimePanel";
    public static readonly string PANEL_DATETIME = "DateTimePanel";
    public static readonly string PANEL_LOCATION = "LocationPanel";
    
    public static readonly string PANEL_NETWORK = "NetworkPanel";
    public static readonly string PANEL_SCENESELECTION = "SceneSelectionPanel";
    public static readonly string PANEL_SNAPSHOT = "SnapshotPanel";
    public static readonly string PANEL_SPHEREMOVEMENT = "SphereMovementPanel";
    
    public static readonly string PANEL_STARINFO = "StarInfoPanel";
    public static readonly string PANEL_TITLE = "TitlePanel";
    public static readonly string PANEL_TOGGLE = "TogglePanel";
    public static readonly string PANEL_VISIBILITY = "VisibilityPanel";
    
    // UI Toggle Button Names:
    public static readonly string BUTTON_TOGGLE_MOVEMENT = "ButtonToggleMovement";
    public static readonly string BUTTON_TOGGLE_DATE = "ButtonToggleDate";
    public static readonly string BUTTON_TOGGLE_DRAW = "ButtonToggleDraw";
    public static readonly string BUTTON_TOGGLE_LOCATION = "ButtonToggleLocation";
    public static readonly string BUTTON_TOGGLE_SNAPSHOT = "ButtonToggleSnapshot";
    public static readonly string BUTTON_TOGGLE_CONSTELLATION = "ButtonToggleConstellation";
    public static readonly string BUTTON_TOGGLE_NETWORK = "ButtonToggleNetwork";
    
    // UI Panel Scene Defaults:
    public static readonly string[] PANELS_ALWAYS = {PANEL_TITLE, PANEL_TOGGLE, PANEL_SCENESELECTION};
    public static readonly string[] PANELS_STARS = {PANEL_CONSTELLATION, PANEL_VISIBILITY, PANEL_SPHEREMOVEMENT};
    public static readonly string[] PANELS_HORIZON =
        {PANEL_CURRENTTIME, PANEL_DATETIME, PANEL_LOCATION, PANEL_SNAPSHOT, PANEL_NETWORK};

    public static readonly string[] PANELS_EARTH = {PANEL_LOCATION, PANEL_NETWORK};

    // UI Toggle Buttons to Disable:
    public static readonly string[] BUTTONS_STARS =
        {BUTTON_TOGGLE_DATE, BUTTON_TOGGLE_DRAW, BUTTON_TOGGLE_LOCATION, BUTTON_TOGGLE_SNAPSHOT};

    public static readonly string[] BUTTONS_EARTH =
        {BUTTON_TOGGLE_DRAW, BUTTON_TOGGLE_MOVEMENT, BUTTON_TOGGLE_CONSTELLATION};

    public static readonly string[] BUTTONS_HORIZON = {BUTTON_TOGGLE_MOVEMENT};
}
