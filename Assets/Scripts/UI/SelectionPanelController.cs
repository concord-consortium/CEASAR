using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionListMode { Constellation = 0, Location = 1, Snapshots = 2 }
public class SelectionPanelController : MonoBehaviour
{
    private ConstellationDropdown _constellationDropdown;
    private CityList _cityList;
    private SnapGrid _snapshotGrid;

    [SerializeField] private GameObject selectionConstellation;
    [SerializeField] private GameObject selectionLocation;
    [SerializeField] private GameObject selectionSnapshots;
    private void OnEnable()
    {
        _constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        _cityList = FindObjectOfType<CityList>();
        _snapshotGrid = FindObjectOfType<SnapGrid>();
        if (_cityList)
        {
            _cityList.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName);
        }
        if (_constellationDropdown)
        {
            _constellationDropdown.InitConstellationNames(DataManager.Instance.ConstellationFullNames, "all");
        }
    }

    public void SetSelection(int modeIdx)
    {
        selectionConstellation.SetActive(false);
        selectionLocation.SetActive(false);
        selectionSnapshots.SetActive(false);

        SelectionListMode mode = (SelectionListMode) modeIdx;
        switch (mode)
        {
            case SelectionListMode.Constellation:
                selectionConstellation.SetActive(true);
                break;
            case SelectionListMode.Location:
                selectionLocation.SetActive(true);
                break;
            case SelectionListMode.Snapshots:
                selectionSnapshots.SetActive(true);
                break;
        }
    }
}
