using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstellationDropdown : MonoBehaviour
{
    public GameObject constellationManager;
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
        if (constellationManager)
        {
            if (change.captionText.text == "all")
            {
                constellationManager.GetComponent<ConstellationManager>().HighlightAllConstellations(true);
            }
            else if (change.captionText.text == "None")
            {
                constellationManager.GetComponent<ConstellationManager>().HighlightAllConstellations(false);
            }
            else
            {
                constellationManager.GetComponent<ConstellationManager>().HighlightSingleConstellation(change.captionText.text);
            }
        }
    }

    public void InitConstellationNames(List<string> constellationNames, string currentConstellation)
    {
        var options = new List<string>(constellationNames);
        // Get dropdown reference in case InitConstellationNames is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        options.Remove("");
        dropdown.AddOptions(options);
        int initialValue = options.IndexOf(currentConstellation);

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
