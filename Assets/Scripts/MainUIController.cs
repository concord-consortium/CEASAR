using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class MainUIController : MonoBehaviour
{
    private DataController dataController;
    private SnapshotsController snapshotsController;

    // main control panel where we slot tools
    public GameObject controlPanel;

    private Vector2 initialPosition;
    private Vector2 hiddenPosition;
    private Vector2 targetPosition;
    private bool movingControlPanel = false;
    private float speed = 750.0f;

    // date and time controls
    private int userYear = 2019;
    private int userHour = 0;
    private int userMin = 0;
    private int userDay = 1;
    public TextMeshProUGUI currentDateTimeText;
    public Toggle setTimeToggle;
    public TMP_InputField yearInput;
    public Slider daySlider;
    public Slider timeSlider;

    // star selection controls
    public GameObject starInfoPanel;

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

    public void Init()
    {
        markersController = SimulationManager.GetInstance().MarkersControllerComponent;
        dataController = SimulationManager.GetInstance().DataControllerComponent;
        sphere = SimulationManager.GetInstance().CelestialSphereObject;

        snapshotsController = FindObjectOfType<SnapshotsController>();
        constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        cityDropdown = FindObjectOfType<CityDropdown>();
        snapshotGrid = FindObjectOfType<SnapGrid>();
        RectTransform controlPanelRect = controlPanel.GetComponent<RectTransform>();
        initialPosition = controlPanelRect.anchoredPosition;
        float hiddenY = controlPanelRect.rect.height * -0.5f + 50f;
        hiddenPosition = new Vector2(controlPanelRect.anchoredPosition.x, hiddenY);
        targetPosition = initialPosition;

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
            foreach (Snapshot snapshot in snapshotsController.snapshots)
            {
                AddSnapshotToGrid(snapshot);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDateTimeText)
        {
            currentDateTimeText.text = dataController.CurrentSimUniversalTime().ToString() + " (UTC)";
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

    public void ToggleShowControlPanel()
    {
        if (targetPosition == initialPosition)
        {
            targetPosition = hiddenPosition;
        }
        else
        {
            targetPosition = initialPosition;
        }
        movingControlPanel = true;
    }

    public void ChangeYear(string newYear)
    {
        userYear = int.Parse(newYear);
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
        dataController.SetUserStartDateTime(calculatedStartDateTime);
    }

    public void ChangeStarSelection(GameObject selectedStar)
    {
        if (starInfoPanel)
        {
            StarComponent starComponent = selectedStar.GetComponent<StarComponent>();
            starInfoPanel.GetComponent<StarInfoPanel>().UpdateStarInfoPanel(starComponent.starData);
            starInfoPanel.GetComponent<WorldToScreenPos>().UpdatePosition(selectedStar);
        }
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
        DateTime snapshotDateTime = dataController.CurrentSimUniversalTime();
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
        yearInput.text = userYear.ToString();
        daySlider.value = userDay;
        timeSlider.value = userHour * 60 + userMin;
    }

    public void DeleteSnapshot(Snapshot deleteSnap)
    {
        snapshotsController.DeleteSnapshot(deleteSnap);
    }

}
