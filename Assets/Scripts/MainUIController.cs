using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MainUIController : MonoBehaviour
{
    // main control panel where we slot tools
    public GameObject controlPanel;

    // date and time controls
    private int userYear = 2019;
    private int userHour = 0;
    private int userMin = 0;
    private int userDay = 1;
    public TextMeshProUGUI currentDateTimeText;

    // star selection controls
    public GameObject starInfoPanel;

    public DataController dataController;

    private Vector2 initialPosition;
    private Vector2 hiddenPosition;
    private Vector2 targetPosition;
    private bool movingControlPanel = false;
    private float speed = 750.0f;
    public GameObject constellationDropdown;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform controlPanelRect = controlPanel.GetComponent<RectTransform>();
        initialPosition = controlPanelRect.anchoredPosition;
        float hiddenY = controlPanelRect.rect.height * -0.5f + 50f;
        hiddenPosition = new Vector2(controlPanelRect.anchoredPosition.x, hiddenY);
        targetPosition = initialPosition;
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
            float step =  speed * Time.deltaTime; // calculate distance to move
            Vector2 currentPos = controlPanel.GetComponent<RectTransform>().anchoredPosition;
            Vector2 newPos = Vector3.MoveTowards(currentPos, targetPosition, step);
            controlPanel.GetComponent<RectTransform>().anchoredPosition = newPos;
            if (Vector2.Distance(newPos, targetPosition) < 0.001f)
            {
                movingControlPanel = false;
            }
        }
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
        userDay = (int) newDay;
        CalculateUserDateTime();
    }

    public void ChangeTime(float newTime)
    {
        userHour = (int) newTime / 60;
        userMin = (int) (newTime % 60);
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

}
