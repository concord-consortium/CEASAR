using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationRateDropdown : MonoBehaviour
{
    private DataController dataController;
    TMP_Dropdown dropdown;

    void Start()
    {
        dataController = SimulationManager.Instance.DataControllerComponent;
        dropdown = GetComponent<TMP_Dropdown>();
        //Add listener for when the value of the Dropdown changes, to take action
        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (dataController)
        {
            string newScale = change.captionText.text.Remove(change.captionText.text.Length - 1, 1);
            dataController.SetSimulationTimeScale((float)System.Convert.ToDouble(newScale));
        }
    }

}
