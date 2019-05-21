using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CityDropdown : MonoBehaviour
{
    public GameObject dataController;
    TMP_Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        // Add listener for when the value of the Dropdown changes
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (dataController)
        {
            dataController.GetComponent<DataController>().ChangeCity(change.captionText.text);
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
}
