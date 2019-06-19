using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityDropdown : MonoBehaviour
{
    private DataController dataController;
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
            dataController.SetSelectedCity(change.captionText.text);
        }
    }

    public void InitCityNames(List<string> cityNames, string currentCity)
    {
        // Get dropdown reference in case InitCityNames is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(cityNames);
        int initialValue = cityNames.IndexOf(currentCity);
        if (initialValue < 0)
        {
            initialValue = 0;
        }
        dropdown.value = initialValue;
    }

    public void UpdateCitySelection(string location)
    {
        dropdown = GetComponent<TMP_Dropdown>();
        List<TMP_Dropdown.OptionData> dropdownOptions = dropdown.options;
        int dropdownValue = dropdownOptions.FindIndex(el => el.text == location);
        if (dropdownValue < 0)
        {
            dropdownValue = 0;
        }
        dropdown.value = dropdownValue;
    }
}
