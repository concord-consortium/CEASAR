using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityDropdown : MonoBehaviour
{
    private DataController dataController;
    private List<string> allCityNames;
    TMP_Dropdown dropdown;

    void Start()
    {
        dataController = SimulationManager.GetInstance().DataControllerComponent;
        dropdown = GetComponent<TMP_Dropdown>();
        // Add listener for when the value of the Dropdown changes
        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (dataController)
        {
            SimulationEvents.GetInstance().LocationSelected.Invoke(change.captionText.text);
        }
    }

    // When the location is set elsewhere (from a pushpin interaction etc) sync the drop down to match if possible
    public void SetCity(string city)
    {
        dropdown.SetValueWithoutNotify(allCityNames.IndexOf(city));
    }

    public void InitCityNames(List<string> cityNames, string currentCity)
    {
        if (allCityNames == null || allCityNames.Count == 0)
        {
            // Get dropdown reference in case InitCityNames is called before Start
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.AddOptions(cityNames);
            int initialValue = cityNames.IndexOf(currentCity);
            if (initialValue < 0)
            {
                initialValue = 0;
            }

            dropdown.SetValueWithoutNotify(initialValue);
            // cache the list of names
            allCityNames = cityNames;
        }
        else
        {
            SetCity(currentCity);
        }
    }

}
