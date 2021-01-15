using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionListMode { Constellation = 0, Location = 1, Snapshots = 2 }
public class SelectionPanelController : MonoBehaviour
{
    private ConstellationDropdown _constellationDropdown;
    private ConstellationList _constellationList;
    private CityList _cityList;
    private CityDropdown _cityDropdown;
    private SnapGrid _snapshotGrid;

    [SerializeField] private GameObject selectionConstellation;
    [SerializeField] private GameObject selectionLocation;
    [SerializeField] private GameObject selectionSnapshots;
    private void OnEnable()
    {
        _constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        _constellationList = FindObjectOfType<ConstellationList>();
        _cityList = FindObjectOfType<CityList>();
        _cityDropdown = FindObjectOfType<CityDropdown>();
        _snapshotGrid = FindObjectOfType<SnapGrid>();
        if (_cityList)
        {
            Debug.Log("found city list");
            _cityList.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName);
        }

        if (_cityDropdown)
        {
            Debug.Log("found city drop down");
            _cityDropdown.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName);
        }
        if (_constellationDropdown)
        {
            _constellationDropdown.InitConstellationNames(DataManager.Instance.ConstellationFullNames, "all");
        }

        if (_constellationList)
        {
            _constellationList.InitConstellations(DataManager.Instance.ConstellationFullNames, "all");
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
