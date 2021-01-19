using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }
    private string _currentSceneName
    {
        get { return SceneManager.GetActiveScene().name; }
    }
    private DataController dataController
    {
        get {
            DataController dController = manager.DataControllerComponent;
            if (dController == null)
            {
                CCDebug.Log("Missing Data Controller", LogLevel.Warning, LogMessageCategory.All);
            }
            return dController;
        }
    }
    private SnapshotsController snapshotsController;
    private GameObject annotationsObject;
    private bool hideAnnotations = false;
    private bool hasSetNorthPin = false;
    private SnapGrid _snapshotGrid;
    
    public GameObject drawModeIndicator;
    [SerializeField] private GameObject menuContainerObject;
    [SerializeField] private GameObject showHideMenuToggle;
    private string _menuShowIcon = "≡";
    private string _menuHideIcon = "X";
    private bool showMainMenu = true;
    public void ToggleMainMenu()
    {
        showMainMenu = !showMainMenu;
        menuContainerObject.SetActive(showMainMenu);
        if (showMainMenu)
        {
            // use SendMessage because text component on canvas ui vs world ui is different
            showHideMenuToggle.SendMessage("SetText", _menuHideIcon);
        }
        else showHideMenuToggle.SendMessage("SetText", _menuShowIcon);
    }
    private bool isDrawing = false;
    public bool IsDrawing {
        get { return isDrawing; }
        set { 
            isDrawing = value;
            if (drawModeIndicator) drawModeIndicator.SetActive(isDrawing);
            if (!isDrawing)
            {
                AnnotationTool annotationTool = FindObjectOfType<AnnotationTool>();
                if (annotationTool)
                {
                    annotationTool.EndDrawingMode();
                }
            }
        }
    }

    private bool isPinningLocation = true;
    public bool IsPinningLocation {
        get { return isPinningLocation; }
        set { 
            isPinningLocation = value;
        }
    }

    private bool _hasCompletedSetup = false;
    
    private void Awake()
    {
        // DontDestroyOnLoad(this.transform.root.gameObject);
    }
    private void Start()
    {
        // Should only happen once, but just in case
        if (!_hasCompletedSetup)
        {
            Init();
            _hasCompletedSetup = true;
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearStarSelection();
        HideNorthPin();
        HideAnnotations(scene.name != SimulationConstants.SCENE_HORIZON);
    }
    
    void OnDisable()
    {
        events.PushPinSelected.RemoveListener(updateOnPinSelected);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Init()
    {
        snapshotsController = GetComponent<SnapshotsController>();
        snapshotsController.Init();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    public void ToggleDrawMode()
    {
        if (!hideAnnotations)
        {
            IsDrawing = !IsDrawing;
            events.DrawMode.Invoke(IsDrawing);
        }
    }

    public void HideAnnotations(bool hide)
    {
        if (hide && isDrawing)
        {
            // turn off drawing
            ToggleDrawMode();
        }
        // If we hide annotations, we can't draw til we turn them back on
        hideAnnotations = hide;
        SceneLoader loader = FindObjectOfType<SceneLoader>();
        
        int layerMaskAnnotations = 19;
        if (hideAnnotations)
        {
            LayerMask newLayerMask = loader.DefaultSceneCameraLayers & ~(1 << layerMaskAnnotations);
            Camera.main.cullingMask = newLayerMask;
        }
        else
        {
            LayerMask newLayerMask = loader.DefaultSceneCameraLayers | (1 << layerMaskAnnotations);
            Camera.main.cullingMask = newLayerMask;
        }
    }
    public void JumpToCrashSite()
    {
        // This is now used to change the view in Horizon mode to your crash site pin
        Pushpin crashPin = manager.CrashSiteForGroup;
        if (crashPin == null) crashPin = UserRecord.GroupPins[manager.GroupName];
        manager.JumpToPin(crashPin);
        SimulationEvents.Instance.LocationSelected.Invoke(crashPin.LocationName);
        SimulationEvents.Instance.PushPinSelected.Invoke(crashPin);
        if (SceneManager.GetActiveScene().name != "Horizon")
        {
            SceneManager.LoadScene("Horizon");
        }
    }
    public void ChangeYear(int yearChange)
    {
        // Setting simulation time updates Local User Pin
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddYears(yearChange);
        events.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
    }
    
    public void ChangeMonth(int monthChange)
    {
        // Setting simulation time updates Local User Pin
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddMonths(monthChange);
        events.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
    }

    public void ChangeDay(int dayChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddDays(dayChange);
        events.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
    }

    public void ChangeTime(int hourChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddHours(hourChange);
        events.PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
    }
    public void ClearStarSelection()
    {
        manager.CurrentlySelectedStar = null;
        manager.ConstellationsControllerComponent.HighlightAllConstellations(manager.ConstellationsControllerComponent.SceneShouldShowConstellations);
    }

    public void SetSimulationTimeToNow()
    {
        Pushpin currentTimeForLocation = new Pushpin(DateTime.UtcNow, manager.CurrentLatLng, manager.CurrentLocationName);
        RestoreSnapshotOrPin(currentTimeForLocation);
    }

    public void ToggleRunSimulation()
    {
        if (dataController)
        {
            dataController.ToggleRunSimulation();
        }
    }
 

    public void CreateSnapshot()
    {
        // Update local player pin to remote players - unsure if this is needed
        // calculateUserDateTime(true);
        // get values from simulation manager
        Pushpin newPin = new Pushpin(manager.CurrentSimulationTime, manager.CurrentLatLng, manager.CurrentLocationName);
        manager.LocalUserSnapshots.Add(newPin);
        // add a snapshot to the controller
        snapshotsController.SaveSnapshot(newPin, manager.LocalUserSnapshots.Count - 1);
        // add snapshot to dropdown list
        // AddSnapshotToGrid(newPin);
    }

    public void RestoreSnapshot(Pushpin snapshot)
    {
        // int snapshotIndex = manager.LocalUserSnapshots.FindIndex(el => el.Location == snapshot.Location && el.SelectedDateTime == snapshot.SelectedDateTime);
        // // user restores snapshot from UI
        // Pushpin snap = manager.LocalUserSnapshots[snapshotIndex];
        CCDebug.Log(snapshot.SelectedDateTime + " " + snapshot.LocationName + " " + snapshot.Location, LogLevel.Info, LogMessageCategory.Event);
        SimulationEvents.Instance.SnapshotLoaded.Invoke(snapshot);
        RestoreSnapshotOrPin(snapshot);
    }

    public void RestoreSnapshotOrPin(Pushpin pin)
    {
        // Update the manager so everything is ready to read the new pin values
        manager.JumpToPin(pin);
        
        // updateTimeSlidersFromPin(pin);
        // updateStarInfoPanel();
        
        // update local player perspective on select
        events.PushPinSelected.Invoke(pin);
        // broadcast the update to current perspective to the network
        events.PushPinUpdated.Invoke(pin, manager.LocalPlayerLookDirection);
    }
    
      public void DeleteSnapshot(Pushpin deleteSnap)
    {
        Pushpin match = manager.LocalUserSnapshots.Find(p => p.Equals(deleteSnap));
        CCDebug.Log($"Deleting snapshot: {match}");
        manager.LocalUserSnapshots.Remove(match);
        snapshotsController.DeleteSnapshot(match);
    }

    private void updateOnPinSelected(Pushpin pin)
    {
        if (FindObjectOfType<LocationPanel>())
        {
            CCDebug.Log("Updating drop down" + pin.Location + " " + pin.LocationName, LogLevel.Info, LogMessageCategory.Event);
            FindObjectOfType<LocationPanel>().UpdateLocationPanel(pin);
        }
    }

    public void HideNorthPin()
    {
        if (hasSetNorthPin)
        {
            GameObject northPin = GameObject.Find("NorthPin");
            if (northPin != null)
            {
                if (SceneManager.GetActiveScene().name != SimulationConstants.SCENE_HORIZON)
                {
                    northPin.transform.position = new Vector3(0, 0.1f, -100000);
                }
                else
                {
                    northPin.transform.position = new Vector3(0, 0.1f, 0);
                }
                
            }
        }
    }
    public void DropNorthPin()
    {
        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON)
        {
            GameObject northPin = GameObject.Find("NorthPin");
            if (northPin != null)
            {
                northPin.transform.position = new Vector3(0, 0.1f, 0);
                northPin.transform.localRotation = Quaternion.Euler(0, manager.LocalPlayerLookDirection.y, 0);
                hasSetNorthPin = true;
                SimulationEvents.Instance.PlayerNorthPin.Invoke(manager.LocalPlayerLookDirection.y);
                DontDestroyOnLoad(northPin);
            }
        }
    }
    
    public void ChangeStarQuantity(float newVal)
    {
        if (dataController)
        {
            if (newVal > 0) dataController.ShowMoreStars();
            else dataController.ShowFewerStars();
        }
    }
    
    public void LoadEarthScene()
    {
        if (_currentSceneName != SimulationConstants.SCENE_EARTH)
        {
            SceneManager.LoadScene(SimulationConstants.SCENE_EARTH);
        }
    }
    public void LoadHorizonScene()
    {
        if (_currentSceneName != SimulationConstants.SCENE_HORIZON)
        {
            SceneManager.LoadScene(SimulationConstants.SCENE_HORIZON);
        }
    }
    public void LoadStarsScene()
    {
        if (_currentSceneName != SimulationConstants.SCENE_STARS)
        {
            SceneManager.LoadScene(SimulationConstants.SCENE_STARS);
        }
    }
    public void QuitApplication()
    {
        CCDebug.Log("Quit application called", LogLevel.Warning, LogMessageCategory.UI);
#if !UNITY_WEBGL && !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
