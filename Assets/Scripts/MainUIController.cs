using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class MainUIController : MonoBehaviour
{
    private int userYear = 2019;
    private int userHour = 0;
    private int userMin = 0;
    private int userDay = 1;
    public TextMeshProUGUI currentDateTimeText;

    public DataController dataController;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (currentDateTimeText)
        {
            currentDateTimeText.text = dataController.CurrentSimUniversalTime().ToString() + " (UTC)";
        }
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
}
