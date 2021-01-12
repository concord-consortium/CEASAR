using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionListMode { Constellation, Location, Snapshots }
public class SelectionPanelController : MonoBehaviour
{
    private ConstellationDropdown _constellationDropdown;
    private CityDropdown _cityDropdown;
    private SnapGrid _snapshotGrid;
    
    [SerializeField] private Canvas _selectionCanvas;

    [SerializeField] private GameObject selectionConstellation;
    [SerializeField] private GameObject selectionLocation;
    [SerializeField] private GameObject selectionSnapshots;
    private void OnEnable()
    {
        
        if (!_selectionCanvas) _selectionCanvas = transform.parent.GetComponent<Canvas>();
        // Set camera mode (will need to change for VR)
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_EDITOR
        Camera cam = Camera.main;
        _selectionCanvas.worldCamera = cam;
        _selectionCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        _selectionCanvas.planeDistance = 1;
#endif
        _constellationDropdown = FindObjectOfType<ConstellationDropdown>();
        _cityDropdown = FindObjectOfType<CityDropdown>();
        _snapshotGrid = FindObjectOfType<SnapGrid>();
        if (_cityDropdown)
        {
            _cityDropdown.InitCityNames(DataManager.Instance.CityNames, SimulationManager.Instance.CurrentLocationName);
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
