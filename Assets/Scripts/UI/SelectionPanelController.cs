using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SelectionListMode { Constellation = 0, Location = 1, Snapshots = 2 }
public class SelectionPanelController : MonoBehaviour
{
    [SerializeField] private ConstellationDropdown constellationDropdown;
    [SerializeField] private ConstellationList constellationList;
    [SerializeField] private CityList cityList;
    [SerializeField] private CityDropdown cityDropdown;
    [SerializeField] private SnapGrid snapshotGrid;

    [SerializeField] private GameObject selectionConstellation;
    [SerializeField] private GameObject selectionLocation;
    [SerializeField] private GameObject selectionSnapshots;
    private void OnEnable()
    {
        if (!constellationDropdown) constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        if (!constellationList) constellationList = FindObjectOfType<ConstellationList>();
        if (!cityList) cityList = FindObjectOfType<CityList>();
        if (!cityDropdown) cityDropdown = FindObjectOfType<CityDropdown>();
        if (!snapshotGrid) snapshotGrid = FindObjectOfType<SnapGrid>();
        if (cityList && !cityList.HasCompletedSetup)
        {
            StartCoroutine(cityList.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName));
        }

        if (cityDropdown)
        {
            Debug.Log("found city drop down");
            cityDropdown.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName);
        }
        
        string initialConstellation =
            SceneManager.GetActiveScene().name == SimulationConstants.SCENE_STARS ? "all" : "none";
        if (constellationDropdown)
        {
            constellationDropdown.InitConstellationNames(DataManager.Instance.ConstellationFullNames, initialConstellation);
        }

        if (constellationList && !constellationList.HasCompletedSetup)
        {
            StartCoroutine(constellationList.InitConstellations(DataManager.Instance.ConstellationFullNames, initialConstellation));
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
