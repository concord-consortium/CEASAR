using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SelectionListMode { Constellation = 0, Location = 1, Snapshots = 2, Instructions = 3 }
public class SelectionPanelController : MonoBehaviour
{
    [SerializeField] private ConstellationList constellationList;
    [SerializeField] private CityList cityList;
    [SerializeField] private SnapGrid snapshotGrid;

    [SerializeField] private GameObject selectionConstellation;
    [SerializeField] private GameObject selectionLocation;
    [SerializeField] private GameObject selectionSnapshots;
    [SerializeField] private GameObject instructions;

    public bool ForceUpdateLists = false;
    void OnEnable()
    {
        if (!constellationList) constellationList = FindObjectOfType<ConstellationList>();
        if (!cityList) cityList = FindObjectOfType<CityList>();
        if (!snapshotGrid) snapshotGrid = FindObjectOfType<SnapGrid>();
        if (!instructions) instructions = GameObject.Find("InstructionPanel");
        if (cityList && !cityList.HasCompletedSetup)
        {
            StartCoroutine(cityList.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName, ForceUpdateLists));
        }
        
        string initialConstellation =
            SceneManager.GetActiveScene().name == SimulationConstants.SCENE_STARS ? "all" : "none";


        if (constellationList && !constellationList.HasCompletedSetup)
        {
            StartCoroutine(constellationList.InitConstellations(DataManager.Instance.ConstellationFullNames, initialConstellation, ForceUpdateLists));
        }
    }

    public void SetSelection(int modeIdx)
    {
        selectionConstellation.SetActive(false);
        selectionLocation.SetActive(false);
        selectionSnapshots.SetActive(false);
        if (instructions != null) instructions.SetActive(false);

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
            case SelectionListMode.Instructions:
                if (instructions == null) this.gameObject.SetActive(false);
                else instructions.SetActive(true);
                break;
        }
    }
}
