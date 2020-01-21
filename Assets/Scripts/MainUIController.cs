using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class MainUIController : MonoBehaviour
{
    private SimulationManager manager;

    public List<GameObject> enabledPanels = new List<GameObject>();
    public List<GameObject> buttonsToDisable = new List<GameObject>();
    private Dictionary<string, GameObject> allPanels = new Dictionary<string, GameObject>();

    private DataController dataController;
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
                    annotationTool.CleanLastLinePosition();
                }
            }
        }
    }
    private void Awake()
    {
        manager = SimulationManager.GetInstance();
    }
    public void Init()
    {
        manager = SimulationManager.GetInstance();
        markersController = manager.MarkersControllerComponent;
        dataController = manager.DataControllerComponent;
        sphere = manager.CelestialSphereObject;
        allPanels = new Dictionary<string, GameObject>();
        foreach (Transform t in controlPanel.transform) {
            allPanels.Add(t.name, t.gameObject);
        }
        positionActivePanels();

        snapshotsController = FindObjectOfType<SnapshotsController>();
        constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        cityDropdown = FindObjectOfType<CityDropdown>();
        snapshotGrid = FindObjectOfType<SnapGrid>();


        if (cityDropdown)
        {
            cityDropdown.InitCityNames(dataController.cities, dataController.SelectedCity);
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
            yearSlider.value = dataController.CurrentSimUniversalTime.Year;
            daySlider.value = dataController.CurrentSimUniversalTime.DayOfYear;
            timeSlider.value = dataController.CurrentSimUniversalTime.Hour * 60 + dataController.CurrentSimUniversalTime.Minute;
        }
        foreach (GameObject buttonToDisable in buttonsToDisable)
        {
            buttonToDisable.GetComponent<Button>().enabled = false;
            buttonToDisable.GetComponent<Image>().color = Color.gray;
        }
    }
    public void AddPanel(GameObject panel)
    {
        if (!enabledPanels.Contains(panel)) enabledPanels.Add(panel);
        positionActivePanels();
    }
    public void RemovePanel(GameObject panel)
    {
        if (enabledPanels.Contains(panel)) enabledPanels.Remove(panel);
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
            if (enabledPanels.Contains(go))
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
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDateTimeText && dataController)
        {
            currentDateTimeText.text = dataController.CurrentSimUniversalTime.ToString() + " (UTC)";
        }

        // allow change of time in all scenes - should work in Earth scene to switch seasons
        double lst;
        if (manager == null) manager = SimulationManager.GetInstance();
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
            if (enabledPanels.Contains(togglePanelObject))
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
            if (!enabledPanels.Contains(togglePanelObject))
                AddPanel(togglePanelObject);
        }
    }
    public void HidePanel(string panelName)
    {
        GameObject togglePanelObject = allPanels[panelName];
        if (togglePanelObject != null)
        {
            if (enabledPanels.Contains(togglePanelObject))
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
            sphere.transform.Rotate(Vector3.right, autoRotateSpeed * Time.deltaTime);
            sphere.transform.Rotate(Vector3.back, autoRotateSpeed * Time.deltaTime);
            sphere.transform.Rotate(Vector3.down, autoRotateSpeed * Time.deltaTime);
        }
    }

    public void Reset()
    {
        sphere.transform.position = new Vector3(0, 0, 0f);
        sphere.transform.localScale = new Vector3(1f, 1f, 1f);
        sphere.transform.rotation = Quaternion.identity;
    }

    public void SetMagnitudeThreshold(float newVal)
    {
        dataController.SetMagnitudeThreshold(newVal);
    }

    public void ToggleUserTime()
    {
        dataController.ToggleUserTime();
    }

    public void ToggleRunSimulation()
    {
        dataController.ToggleRunSimulation();
    }
    public void ChangeCitySelection(string location)
    {
        if (cityDropdown)
        {
            cityDropdown.GetComponent<CityDropdown>().UpdateCitySelection(location);
        }
    }

    public void CreateSnapshot()
    {
        // get values from datacontroller
        DateTime snapshotDateTime = dataController.CurrentSimUniversalTime;
        String location = dataController.SelectedCity;
        // add a snapshot to the controller
        snapshotsController.CreateSnapshot(snapshotDateTime, location);
        // add snapshot to dropdown list
        AddSnapshotToGrid(snapshotsController.snapshots[snapshotsController.snapshots.Count - 1]);
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
        DateTime snapshotDateTime = snapshotsController.snapshots[snapshotIndex].dateTime;
        userYear = snapshotDateTime.Year;
        userDay = snapshotDateTime.DayOfYear;
        userHour = snapshotDateTime.Hour;
        userMin = snapshotDateTime.Minute;
        CalculateUserDateTime();
        String location = snapshotsController.snapshots[snapshotIndex].location;
        ChangeCitySelection(location);
        setTimeToggle.isOn = true;
        yearSlider.value = userYear;
        daySlider.value = userDay;
        timeSlider.value = userHour * 60 + userMin;
    }

    public void DeleteSnapshot(Snapshot deleteSnap)
    {
        snapshotsController.DeleteSnapshot(deleteSnap);
    }
}
