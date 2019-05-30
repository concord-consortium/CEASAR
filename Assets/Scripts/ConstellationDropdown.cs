using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstellationDropdown : MonoBehaviour
{
    public GameObject dataController;
    TMP_Dropdown dropdown;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        //Add listener for when the value of the Dropdown changes, to take action
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (dataController)
        {
            dataController.GetComponent<DataController>().ChangeConstellationHighlight(change.captionText.text);
        }
    }

    public void InitConstellationNames(List<string> constellationNames, string currentConstellation)
    {
        // Get dropdown reference in case InitConstellationNames is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(constellationNames);
        int initialValue = constellationNames.IndexOf(currentConstellation);
        if (initialValue < 0)
        {
            initialValue = 0;
        }
        dropdown.value = initialValue;
    }

    public void UpdateConstellationSelection(string currentConstellation)
    {
        // Get dropdown reference in case InitConstellationNames is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        List<TMP_Dropdown.OptionData> dropdownOptions = dropdown.options;
        int dropdownValue = dropdownOptions.FindIndex(el => el.text == currentConstellation);
        if (dropdownValue < 0)
        {
            dropdownValue = 0;
        }
        dropdown.SetValueWithoutNotify(dropdownValue);
    }
}
