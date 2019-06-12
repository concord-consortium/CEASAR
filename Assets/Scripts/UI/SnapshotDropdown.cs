using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SnapshotDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    private SnapshotsController snapshotsController;
    private MainUIController mainUIController;

    // Start is called before the first frame update
    void Start()
    {
        snapshotsController = FindObjectOfType<SnapshotsController>();
        mainUIController = FindObjectOfType<MainUIController>();
        dropdown = GetComponent<TMP_Dropdown>();
        // Add listener for when the value of the Dropdown changes
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        if (mainUIController)
        {
            mainUIController.RestoreSnapshot(dropdown.value, change.captionText.text);
        }
    }


    public void InitSnapshots(List<string> snapshots)
    {
        // Get dropdown reference in case InitSnapshots is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(snapshots);
        dropdown.value = 0;
    }

    public void AddSnapshot(List<string> snapshots)
    {
        // Get dropdown reference in case InitSnapshots is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.AddOptions(snapshots);
    }
}
