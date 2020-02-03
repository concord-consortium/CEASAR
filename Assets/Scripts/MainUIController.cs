using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class MainUIController : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.GetInstance(); } }
    private SimulationEvents events { get { return SimulationEvents.GetInstance(); } }

    private Dictionary<string, List<GameObject>> enabledPanels = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, List<GameObject>> buttonsToDisable = new Dictionary<string, List<GameObject>>();
    private Dictionary<string, GameObject> allPanels = new Dictionary<string, GameObject>();
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
                Debug.Log("Missing Data Controller");
            }
            return dController;
        }
    }
    private SnapshotsController snapshotsController;

    // main control panel where we slot tools
    public GameObject controlPanel;

    private Vector2 initialPosition;
    private Vector2 hiddenPosition;
    private Vector2 targetPosition;
    private bool movingControlPanel = false;
    private float speed = 750.0f;
    private bool isHidden = false;

    // date and time controls
    private int userYear = DateTime.UtcNow.Year;
    private int userHour = DateTime.UtcNow.Hour;
    private int userMin = DateTime.UtcNow.Minute;
    private int userDay = DateTime.UtcNow.DayOfYear;
    public TextMeshProUGUI currentDateTimeText;
    public Toggle setTimeToggle;
    public Slider daySlider;
    public Slider timeSlider;

    public Slider yearSlider;

    // star selection controls
    private GameObject _starInfoPanel;
    public GameObject starInfoPanel {
        get { 
            if (_starInfoPanel == null)
            {
                _starInfoPanel = allPanels["StarInfoPanel"];
            }
            return _starInfoPanel; 
        }
    }

    private ConstellationDropdown constellationDropdown;

    // city selection controls
    private CityDropdown cityDropdown;

    // handle sphere interaction
    private GameObject sphere;
    public float moveSpeed = .1f;
    public float scaleSpeed = 5f;
    public float maxScale = 100f;
    public float minScale = .025f;
    public float rotateSpeed = 10f;
    private float autoRotateSpeed = 1f;
    private bool rotating = false;
    private MarkersController markersController;
    // snapshots
    private SnapGrid snapshotGrid;

    public GameObject drawModeIndicator;
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

    private bool hasCompletedSetup = false;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        // Should only happen once, but just in case
        if (!hasCompletedSetup)
        {
            Init();
            hasCompletedSetup = true;
        }
    }

    void OnDisable()
    {
        events.LocationChanged.RemoveListener(updateLocationPanel);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Init()
    {
        markersController = manager.MarkersControllerComponent;
        sphere = manager.CelestialSphereObject;
        allPanels = new Dictionary<string, GameObject>();
        foreach (Transform t in controlPanel.transform) {
            allPanels.Add(t.name, t.gameObject);
        }
        
        snapshotsController = FindObjectOfType<SnapshotsController>();
        constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        cityDropdown = FindObjectOfType<CityDropdown>();
        snapshotGrid = FindObjectOfType<SnapGrid>();

        if (cityDropdown)
        {
            cityDropdown.InitCityNames(dataController.cities, manager.CurrentLocationName);
        }
        if (constellationDropdown)
        {
            constellationDropdown.InitConstellationNames(dataController.constellationFullNames, "all");
        }
        if (snapshotsController)
        {
            snapshotsController.Init();
            foreach (Snapshot snapshot in snapshotsController.snapshots)
            {
                AddSnapshotToGrid(snapshot);
            }
        }

        if (yearSlider && daySlider && timeSlider)
        {
            yearSlider.SetValueWithoutNotify(manager.CurrentSimulationTime.Year);
            daySlider.SetValueWithoutNotify(manager.CurrentSimulationTime.DayOfYear);
            timeSlider.SetValueWithoutNotify(manager.CurrentSimulationTime.Hour * 60 + manager.CurrentSimulationTime.Minute);
            setTimeToggle.SetIsOnWithoutNotify(manager.UseCustomSimulationTime);
            userYear = manager.CurrentSimulationTime.Year;
            userDay = manager.CurrentSimulationTime.DayOfYear;
            userHour = manager.CurrentSimulationTime.Hour;
            userMin = manager.CurrentSimulationTime.Minute;
        }
        
        if (starInfoPanel) starInfoPanel.GetComponent<StarInfoPanel>().Setup(this);
        // Listen to any relevant events
        events.LocationChanged.AddListener(updateLocationPanel);
        
        
        // Setup panels for each scene
        List<GameObject> horizonPanels = new List<GameObject>();
        foreach (string panelName in SimulationConstants.PANELS_HORIZON.Concat(SimulationConstants.PANELS_ALWAYS))
        {
            horizonPanels.Add(GameObject.Find(panelName));
        }
        List<GameObject> starsPanels = new List<GameObject>();
        foreach (string panelName in SimulationConstants.PANELS_STARS.Concat(SimulationConstants.PANELS_ALWAYS))
        {
            starsPanels.Add(GameObject.Find(panelName));
        }
        List<GameObject> earthPanels = new List<GameObject>();
        foreach (string panelName in SimulationConstants.PANELS_EARTH.Concat(SimulationConstants.PANELS_ALWAYS))
        {
            earthPanels.Add(GameObject.Find(panelName));
        }
        enabledPanels.Add(SimulationConstants.SCENE_HORIZON, horizonPanels);
        enabledPanels.Add(SimulationConstants.SCENE_EARTH, earthPanels);
        enabledPanels.Add(SimulationConstants.SCENE_STARS, starsPanels);

        List<GameObject> horizonToggleButtonsToDisable = new List<GameObject>();
        foreach (string toggleButton in SimulationConstants.BUTTONS_HORIZON)
        {
            horizonToggleButtonsToDisable.Add(GameObject.Find(toggleButton));
        }
        List<GameObject> earthToggleButtonsToDisable = new List<GameObject>();
        foreach (string toggleButton in SimulationConstants.BUTTONS_EARTH)
        {
            earthToggleButtonsToDisable.Add(GameObject.Find(toggleButton));
        }
        List<GameObject> starsToggleButtonsToDisable = new List<GameObject>();
        foreach (string toggleButton in SimulationConstants.BUTTONS_STARS)
        {
            starsToggleButtonsToDisable.Add(GameObject.Find(toggleButton));
        }
        buttonsToDisable.Add(SimulationConstants.SCENE_HORIZON, horizonToggleButtonsToDisable);
        buttonsToDisable.Add(SimulationConstants.SCENE_EARTH, earthToggleButtonsToDisable);
        buttonsToDisable.Add(SimulationConstants.SCENE_STARS, starsToggleButtonsToDisable);
        
        positionActivePanels();
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        positionActivePanels();
    }
    
    public void AddPanel(GameObject panel)
    {
        if (!enabledPanels[_currentSceneName].Contains(panel)) enabledPanels[_currentSceneName].Add(panel);
        positionActivePanels();
    }
    public void RemovePanel(GameObject panel)
    {
        if (enabledPanels[_currentSceneName].Contains(panel)) enabledPanels[_currentSceneName].Remove(panel);
        positionActivePanels();
    }

    void positionActivePanels()
    {
        // reset menu hide/show
        if (isHidden) controlPanel.GetComponent<RectTransform>().anchoredPosition = initialPosition;

        foreach (Transform t in controlPanel.transform)
        {
            t.gameObject.SetActive(false);
        }

        // UI sub-panels are added to the enabledPanels list and then ordered and positioned
        // vertically starting at the bottom of the Main UI panel (add or remove from list to
        // enable or disable)
        float panelPosition = 0f;
        float totalPanelHeight = 0f;

        // Add the panels in reverse order, so the title is always on top
        // and the child panels are in a consistent order
        for (int i = allPanels.Count - 1; i >= 0; i--)
        {
            GameObject go = allPanels.Values.ToArray()[i];
            if (enabledPanels[_currentSceneName].Contains(go))
            {
                go.SetActive(true);
                RectTransform goRect = go.GetComponent<RectTransform>();
                panelPosition += goRect.rect.height * .5f;
                goRect.anchoredPosition = new Vector2(0, panelPosition);
                panelPosition += goRect.rect.height * .5f;
                totalPanelHeight = totalPanelHeight + goRect.rect.height;
            }
        }

        RectTransform controlPanelRect = controlPanel.GetComponent<RectTransform>();
        initialPosition = controlPanelRect.anchoredPosition;
        float hiddenY = controlPanelRect.rect.height * .5f - totalPanelHeight + 50f;
        hiddenPosition = new Vector2(controlPanelRect.anchoredPosition.x, hiddenY);
        targetPosition = initialPosition;
        
        // buttons
        foreach (GameObject toggleButton in GameObject.FindGameObjectsWithTag("ToggleButton"))
        {
            toggleButton.GetComponent<Button>().enabled = true;
            toggleButton.GetComponent<Image>().color = Color.white;
        }
        foreach (GameObject buttonToDisable in buttonsToDisable[_currentSceneName]) 
        {
            buttonToDisable.GetComponent<Button>().enabled = false;
            buttonToDisable.GetComponent<Image>().color = Color.gray;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDateTimeText && dataController)
        {
            currentDateTimeText.text = manager.CurrentSimulationTime.ToString() + " (UTC)";
        }

        // allow change of time in all scenes - should work in Earth scene to switch seasons
        double lst;
        if (setTimeToggle)
        {
            manager.UseCustomSimulationTime = setTimeToggle.isOn;
        }

        // Move our position a step closer to the target.
        if (movingControlPanel)
        {
            float step = speed * Time.deltaTime; // calculate distance to move
            Vector2 currentPos = controlPanel.GetComponent<RectTransform>().anchoredPosition;
            Vector2 newPos = Vector3.MoveTowards(currentPos, targetPosition, step);
            controlPanel.GetComponent<RectTransform>().anchoredPosition = newPos;
            if (Vector2.Distance(newPos, targetPosition) < 0.001f)
            {
                movingControlPanel = false;
            }
        }

        handleAutoRotation();
    }

    public void TogglePanel(string panelName)
    {
        
        GameObject togglePanelObject = allPanels[panelName];
        if (togglePanelObject != null)
        {
            if (enabledPanels[_currentSceneName].Contains(togglePanelObject))
                RemovePanel(togglePanelObject);
            else
                AddPanel(togglePanelObject);
        }
    }
    public void ShowPanel(string panelName)
    {
        GameObject togglePanelObject = allPanels[panelName];
        if (togglePanelObject != null)
        {
            if (!enabledPanels[_currentSceneName].Contains(togglePanelObject))
                AddPanel(togglePanelObject);
        }
    }
    public void HidePanel(string panelName)
    {
        GameObject togglePanelObject = allPanels[panelName];
        if (togglePanelObject != null)
        {
            if (enabledPanels[_currentSceneName].Contains(togglePanelObject))
                RemovePanel(togglePanelObject);
        }
    }
    public void ToggleShowControlPanel()
    {
        if (targetPosition == initialPosition)
        {
            targetPosition = hiddenPosition;
            isHidden = true;
        }
        else
        {
            targetPosition = initialPosition;
            isHidden = false;
        }
        movingControlPanel = true;
    }

    public void ToggleDrawMode()
    {
        IsDrawing = !IsDrawing;
        events.DrawMode.Invoke(IsDrawing);
    }

    public void JumpToMyPin()
    {
        // This is now used to change the view in Horizon mode to your current pin
        // Or to take you to the horizon for your current pin from Earth view
        Pushpin pin = manager.LocalUserPin;
        Debug.Log(pin);
        manager.UseCustomSimulationTime = true;
        manager.CurrentSimulationTime = pin.SelectedDateTime;
        manager.Currentlocation = pin.Location;
        if (SceneManager.GetActiveScene().name != "Horizon")
        {
            SceneManager.LoadScene("Horizon");
        }
    }
    public void ChangeYear(float newYear)
    {
        userYear = (int)newYear;
        CalculateUserDateTime();
    }

    public void ChangeDay(float newDay)
    {
        userDay = (int)newDay;
        CalculateUserDateTime();
    }

    public void ChangeTime(float newTime)
    {
        userHour = (int)newTime / 60;
        userMin = (int)(newTime % 60);
        CalculateUserDateTime();
    }

    private void CalculateUserDateTime()
    {
        DateTime calculatedStartDateTime = new DateTime(userYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        calculatedStartDateTime = calculatedStartDateTime.AddDays(userDay - 1);
        calculatedStartDateTime = calculatedStartDateTime.AddHours(userHour);
        calculatedStartDateTime = calculatedStartDateTime.AddMinutes(userMin);
        manager.CurrentSimulationTime = calculatedStartDateTime;
        if (manager.LocalUserPin != null)
        {
            manager.LocalUserPin.SelectedDateTime = calculatedStartDateTime;
        }
        else
        {
            Pushpin p = new Pushpin();
            p.SelectedDateTime = calculatedStartDateTime;
            p.Location = manager.Currentlocation;
            manager.LocalUserPin = p;
            Debug.Log(p);
        }
    }

    public void ChangeStarSelection(GameObject selectedStar)
    {
        // make sure it's visible
        ShowPanel("StarInfoPanel");

        if (starInfoPanel && selectedStar)
        {
            StarComponent starComponent = selectedStar.GetComponent<StarComponent>();
            manager.CurrentlySelectedStar = starComponent;
            starInfoPanel.GetComponent<StarInfoPanel>().UpdateStarInfoPanel();
        }
    }
    public void ClearStarSelection()
    {
        manager.CurrentlySelectedStar = null;
        manager.ConstellationsControllerComponent.HighlightAllConstellations(true);
        HidePanel("StarInfoPanel");
    }

    public void ChangeConstellationHighlight(string highlightConstellation)
    {
        if (constellationDropdown)
        {
            constellationDropdown.GetComponent<ConstellationDropdown>().UpdateConstellationSelection(highlightConstellation);
        }
    }
    
    #region Sphere Movement
    public void MoveLeft()
    {
        Vector3 pos = sphere.transform.position;
        pos.x -= Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void MoveRight()
    {
        Vector3 pos = sphere.transform.position;
        pos.x += Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void MoveDown()
    {
        Vector3 pos = sphere.transform.position;
        pos.y -= Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void MoveUp()
    {
        Vector3 pos = sphere.transform.position;
        pos.y += Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void MoveBack()
    {
        Vector3 pos = sphere.transform.position;
        pos.z += Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void MoveForward()
    {
        Vector3 pos = sphere.transform.position;
        pos.z -= Time.deltaTime + moveSpeed;
        sphere.transform.position = pos;
    }

    public void DecreaseScale()
    {
        if (sphere.transform.localScale.x > minScale)
        {
            float scaleIncrement = sphere.transform.localScale.x * .25f * Time.deltaTime;
            sphere.transform.localScale -= new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
        }
    }
    public void IncreaseScale()
    {
        if (this.transform.localScale.x < maxScale)
        {
            float scaleIncrement = sphere.transform.localScale.x * .25f * Time.deltaTime;
            sphere.transform.localScale += new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
        }
    }

    public void RotateYAxisUp()
    {
        sphere.transform.Rotate(Vector3.down, rotateSpeed * Time.deltaTime);
    }
    public void RotateYAxisDown()
    {
        sphere.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
    public void RotateXAxisLeft()
    {
        sphere.transform.Rotate(Vector3.left, rotateSpeed * Time.deltaTime);
    }
    public void RotateXAxisRight()
    {
        sphere.transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime);
    }
    public void RotateZAxisForward()
    {
        sphere.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }
    public void RotateZAxisBack()
    {
        sphere.transform.Rotate(Vector3.back, rotateSpeed * Time.deltaTime);
    }

    public void ToggleMarkerVisibility()
    {
        markersController.ShowMarkers(!markersController.markersVisible, !markersController.markersVisible, !markersController.markersVisible);
    }

    public void ToggleAutoRotate()
    {
        rotating = !rotating;
    }

    private void handleAutoRotation()
    {
        if (rotating)
        {
            sphere.transform.Rotate(Vector3.down, autoRotateSpeed * Time.deltaTime);
        }
    }

    
    public void ZoomOut()
    {
        GameObject camObject = Camera.main.gameObject;
        if (camObject.GetComponent<AnimateCamera>() != null)
        {
            camObject.GetComponent<AnimateCamera>().ZoomOut();
        }
    }
    public void ZoomIn()
    {
        GameObject camObject = Camera.main.gameObject;
        if (camObject.GetComponent<AnimateCamera>() != null)
        {
            camObject.GetComponent<AnimateCamera>().ZoomIn();
        }
    }
    
    public void Reset()
    {
        sphere.transform.position = new Vector3(0, 0, 0f);
        sphere.transform.localScale = new Vector3(1f, 1f, 1f);
        sphere.transform.rotation = Quaternion.identity;
        GameObject camObject = Camera.main.gameObject;
        if (camObject.GetComponent<AnimateCamera>() != null)
        {
            camObject.GetComponent<AnimateCamera>().ZoomIn();
        }

        rotating = false;
    }
#endregion

    public void SetMagnitudeThreshold(float newVal)
    {
        if (dataController)
        {
            dataController.SetMagnitudeThreshold(newVal);
        }
        
    }

    public void ToggleUserTime()
    {
        if (dataController)
        {
            dataController.ToggleUserTime();
        }
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
        if (dataController)
        {
            // get values from datacontroller
            DateTime snapshotDateTime = manager.CurrentSimulationTime;
            String location = dataController.currentCity.Name;
            LatLng locationCoordinates = manager.Currentlocation;
            // add a snapshot to the controller
            snapshotsController.CreateSnapshot(snapshotDateTime, location, locationCoordinates);
            // add snapshot to dropdown list
            AddSnapshotToGrid(snapshotsController.snapshots[snapshotsController.snapshots.Count - 1]);
        }
    }

    public void AddSnapshotToGrid(Snapshot snapshot)
    {
        // user chooses to add a new snapshot, update the scroll view grid
        snapshotGrid.AddSnapItem(snapshot);
    }

    public void RestoreSnapshot(Snapshot snapshot)
    {
        int snapshotIndex = snapshotsController.snapshots.FindIndex(el => el.location == snapshot.location && el.dateTime == snapshot.dateTime);
        // user restores snapshot from UI
        Snapshot snap = snapshotsController.snapshots[snapshotIndex];
        Debug.Log(snap.dateTime + " " + snap.location + " " + snap.locationCoordinates);
        
        RestoreSnapshotOrPin(snap.dateTime, snap.location, snap.locationCoordinates);
        //
        // userYear = snapshotDateTime.Year;
        // userDay = snapshotDateTime.DayOfYear;
        // userHour = snapshotDateTime.Hour;
        // userMin = snapshotDateTime.Minute;
        // // Update the current date/time in simulation manager
        // CalculateUserDateTime();
        //
        // // broadcast the change of location
        // events.LocationSelected.Invoke(location);
        // setTimeToggle.isOn = true;
        // yearSlider.value = userYear;
        // daySlider.value = userDay;
        // timeSlider.value = userHour * 60 + userMin;
    }

    public void RestoreSnapshotOrPin(DateTime dt, string locationName, LatLng latLng)
    {
        userYear = dt.Year;
        userDay = dt.DayOfYear;
        userHour = dt.Hour;
        userMin = dt.Minute;
        // Update the current date/time in simulation manager
        CalculateUserDateTime();
        setTimeToggle.isOn = true;
        yearSlider.value = userYear;
        daySlider.value = userDay;
        timeSlider.value = userHour * 60 + userMin;

        // events.LocationSelected.Invoke(locationName);
        events.LocationChanged.Invoke(latLng,locationName);
        events.PushPinSelected.Invoke(latLng, dt);
    }

    public void DeleteSnapshot(Snapshot deleteSnap)
    {
        snapshotsController.DeleteSnapshot(deleteSnap);
    }

    private void updateLocationPanel(LatLng latLng, string description)
    {
        if (FindObjectOfType<LocationPanel>())
        {
            Debug.Log("Updating drop down" + latLng.ToString() + " " + description);
            FindObjectOfType<LocationPanel>().UpdateLocationPanel(latLng, description);
        }

        if (cityDropdown)
        {
            cityDropdown.SetCity(description);
        }
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
