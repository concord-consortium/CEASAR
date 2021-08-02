using System.Collections;
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
    private ConstellationsController constellationsController;
    private GameObject annotationsObject;


    public GameObject drawModeIndicator;
    public GameObject drawModeOffIndicator;
    public GameObject showAnnotationsIndicator;
    public GameObject hideAnnotationsIndicator;
    public TMP_Text annotationsButtonText;

    private bool _hideAnnotations = false;
    // Make this a property so we can have side effect triggered when the value is changed
    private bool hideAnnotations {
        get { return _hideAnnotations; }
        set {
            _hideAnnotations = value;
            if (showAnnotationsIndicator) showAnnotationsIndicator.SetActive(!_hideAnnotations);
            if (hideAnnotationsIndicator) hideAnnotationsIndicator.SetActive(_hideAnnotations);
            if (annotationsButtonText) annotationsButtonText.SetText(_hideAnnotations ? "Show Drawings" : "Hide Drawings");
            SceneLoader loader = FindObjectOfType<SceneLoader>();
            int layerMaskAnnotations = LayerMask.NameToLayer("Annotations"); // should be layer 19
            if (_hideAnnotations)
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
    }
    private SnapGrid _snapshotGrid;


    [SerializeField] private GameObject menuContainerObject;
    [SerializeField] private GameObject informationPanelObject;
    [SerializeField] private GameObject showHideMenuToggle;
    [SerializeField] private GameObject showHideInfoPanelToggle;
    [SerializeField] private GameObject northPinPrefab;
    [SerializeField] private TMP_Text annotateMenuTitle;

    private string _menuShowIcon = "≡";
    private string _menuHideIcon = "X";
    private bool showMainMenu = true;
    private bool showInfoPanel = true;
    private AnnotationTool annotationTool;
    public void ToggleMainMenu()
    {
        showMainMenu = !showMainMenu;
        menuContainerObject.SetActive(showMainMenu);
        if (showMainMenu && showHideMenuToggle != null)
        {
            // use SendMessage because text component on canvas ui vs world ui is different
            showHideMenuToggle.SendMessage("SetText", _menuHideIcon);
        }
        else showHideMenuToggle.SendMessage("SetText", _menuShowIcon);
    }
    public void ToggleInfoPanel()
    {
        showInfoPanel = !showInfoPanel;
        informationPanelObject.SetActive(showInfoPanel);
        if (showInfoPanel && showHideInfoPanelToggle != null)
        {
            // use SendMessage because text component on canvas ui vs world ui is different
            showHideInfoPanelToggle.SendMessage("SetText", _menuHideIcon);
        }
        else showHideInfoPanelToggle.SendMessage("SetText", _menuShowIcon);
    }
    private bool isDrawing = false;
    public bool IsDrawing {
        get { return isDrawing; }
        set {
            isDrawing = value;
            if (drawModeIndicator) drawModeIndicator.SetActive(isDrawing);
            if (drawModeOffIndicator) drawModeOffIndicator.SetActive(!isDrawing);
            if (annotateMenuTitle) annotateMenuTitle.SetText(isDrawing ? "Annotate Menu (Drawing)" : "Annotate Menu");
            if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
            CCDebug.Log("Draw toggled: " + isDrawing + " tool: " + (annotationTool != null), LogLevel.Display, LogMessageCategory.All);
            if (!isDrawing)
            {
                if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
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

    float northPinVerticalOffset = 0.1f;

    private bool _hasCompletedSetup = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.transform.root.gameObject);
        manager.MainMenu = this;
#if UNITY_WSA
        northPinVerticalOffset = -1.5f;
#endif
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
        CheckDisplayNorthPin();
        ToggleAnnotationsVisibility();
    }

    void OnDisable()
    {
        events.PushPinSelected.RemoveListener(updateOnPinSelected);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Init()
    {
        snapshotsController = GetComponent<SnapshotsController>();
        // When running tests, there may be no snapshots controller on this game object
        if (snapshotsController) snapshotsController.Init();
        SceneManager.sceneLoaded += OnSceneLoaded;
        hideAnnotations = false;
    }

    public void ToggleDrawMode()
    {
        IsDrawing = !IsDrawing;
        if (IsDrawing) hideAnnotations = false;
        events.DrawMode.Invoke(IsDrawing);

    }
    public void UndoAnnotation()
    {
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
        if (annotationTool)
        {
            annotationTool.UndoAnnotation();
        }

    }
    public void ToggleAnnotationsVisibility()
    {
        if (isDrawing)
        {
            // turn off drawing
            ToggleDrawMode();
        }
        // If we hide annotations, we can't draw til we turn them back on
        if (SceneManager.GetActiveScene().name != SimulationConstants.SCENE_HORIZON)
        {
            // every other scene we need to hide annotations
            hideAnnotations = true;
        }
        else
        {
            // use this as a toggle within Horizon view
            hideAnnotations = !hideAnnotations;
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
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
    }

    public void ChangeMonth(int monthChange)
    {
        // Setting simulation time updates Local User Pin
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddMonths(monthChange);
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
    }

    public void ChangeDay(int dayChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddDays(dayChange);
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
    }

    public void ChangeHour(int hourChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddHours(hourChange);
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
    }

    public void ChangeMinute(int minuteChange)
    {
        manager.CurrentSimulationTime = manager.CurrentSimulationTime.AddMinutes(minuteChange);
        events.PushPinSelected.Invoke(manager.LocalPlayerPin);
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
        // get values from simulation manager
        Pushpin newPin = new Pushpin(manager.CurrentSimulationTime, manager.CurrentLatLng, manager.CurrentLocationName);
        manager.LocalUserSnapshots.Add(newPin);
        // add a snapshot to the controller
        snapshotsController.SaveSnapshot(newPin, manager.LocalUserSnapshots.Count - 1);
    }

    public void RestoreSnapshot(Pushpin snapshot)
    {
        // user restores snapshot from UI
        CCDebug.Log(snapshot.SelectedDateTime + " " + snapshot.LocationName + " " + snapshot.Location, LogLevel.Info, LogMessageCategory.Event);
        SimulationEvents.Instance.SnapshotLoaded.Invoke(snapshot);
        RestoreSnapshotOrPin(snapshot);
    }

    public void RestoreSnapshotOrPin(Pushpin pin)
    {
        // Update the manager so everything is ready to read the new pin values
        manager.JumpToPin(pin);
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

    public void CheckDisplayNorthPin()
    {
        bool shouldDisplay = SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON;
        GameObject[] northPins = GameObject.FindGameObjectsWithTag("NorthPin");
        // if we've changed scene, place our pin
        if (shouldDisplay && manager.HasSetNorthPin && northPins.Length == 0)
        {
            GameObject northPin = Instantiate(northPinPrefab, new Vector3(0, northPinVerticalOffset, 0), Quaternion.identity);

            northPin.transform.localRotation = Quaternion.Euler(0, manager.PlayerNorthPinDirection, 0);
        }

        if (northPins.Length > 0)
        {
            foreach (GameObject np in northPins)
            {
                np.SetActive(shouldDisplay);
            }
        }
    }
    public void DropNorthPin()
    {
        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON)
        {
            if (!manager.HasSetNorthPin)
            {

                GameObject northPin = Instantiate(northPinPrefab, new Vector3(0, northPinVerticalOffset, 0), Quaternion.identity);
                setNorthPin(northPin);
            }
            else
            {
                // previously placed the pin, now we move it
                GameObject northPin = GameObject.FindGameObjectWithTag("NorthPin");
                if (northPin != null)
                {
                    setNorthPin(northPin);
                }
            }
        }
    }

    private void setNorthPin(GameObject northPin)
    {
        northPin.transform.localRotation = Quaternion.Euler(0, manager.LocalPlayerLookDirection.y, 0);
        manager.PlayerNorthPinDirection = manager.LocalPlayerLookDirection.y;
        SimulationEvents.Instance.PlayerNorthPin.Invoke(manager.LocalPlayerLookDirection.y);
    }

    public void ChangeStarQuantity(float newVal)
    {
        if (dataController)
        {
            if (newVal > 0) dataController.ShowMoreStars();
            else dataController.ShowFewerStars();
        }
    }
    public void ShowAllConstellations()
    {
        if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
        constellationsController.SelectConstellationByName(SimulationConstants.CONSTELLATIONS_ALL);
    }
    public void ShowNoConstellations()
    {
        if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
        constellationsController.SelectConstellationByName(SimulationConstants.CONSTELLATIONS_NONE);
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
        StartCoroutine(waitAndQuitApplication());
#endif
    }

    IEnumerator waitAndQuitApplication()
    {
        NetworkController nc = FindObjectOfType<NetworkController>();
        if (nc != null) nc.Disconnect();

        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    private void OnApplicationQuit()
    {

    }
}
