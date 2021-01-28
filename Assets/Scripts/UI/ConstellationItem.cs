using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ConstellationItem : MonoBehaviour
{
    ConstellationsController constellationsController;
    [SerializeField] private GameObject selectedIcon;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text constellationLabel;
    private string _constellationName;

    private bool _isSelected = false;
    private bool _setupComplete = false;
    
    private void OnEnable()
    {
        initExisting();
    }

    private void OnApplicationQuit()
    {
        SimulationEvents.Instance.StarSelected.RemoveAllListeners();
        SimulationEvents.Instance.ConstellationSelected.RemoveAllListeners();
        selectButton.onClick = null;
    }

    public void Init(string constellationName, bool selected)
    {
        gameObject.name = constellationName;
        _constellationName = constellationName;
        constellationLabel.SetText(constellationName);
        _isSelected = selected;
        finishSetup();
    }
    /// <summary>
    /// For lists of constellations that come pre-bundled in prefab form (for easier VR integration)
    /// </summary>
    void initExisting()
    {
        _constellationName = gameObject.name;
        _isSelected = SimulationManager.Instance.CurrentlySelectedConstellation == _constellationName;
        finishSetup();
    }

    void finishSetup()
    {
        if (!constellationsController) constellationsController = FindObjectOfType<ConstellationsController>();
        selectedIcon.SetActive(_isSelected);
        if (!_setupComplete)
        {
            selectButton.onClick.AddListener(() => selectThisConstellation());
            SimulationEvents.Instance.StarSelected.AddListener((star) => onStarSelected());
            SimulationEvents.Instance.ConstellationSelected.AddListener((constellationSelectedName) =>
                onSelectConstellation(constellationSelectedName));
            _setupComplete = true;
        }
    }
    private void selectThisConstellation()
    {
        if (SimulationManager.Instance.CurrentlySelectedConstellation == _constellationName) return;
        else
        {
            if (!_isSelected)
            {
                constellationsController.SelectConstellationByName(_constellationName);
                _isSelected = true;
                selectedIcon.SetActive(true);
            }
        }
    }
    private void onSelectConstellation(string cname)
    {
        _isSelected = cname == _constellationName;
        
        selectedIcon.SetActive(_isSelected || cname == SimulationConstants.CONSTELLATIONS_ALL);
    }
    
    private void onStarSelected()
    {
        
        if (SimulationManager.Instance.CurrentlySelectedConstellation == SimulationConstants.CONSTELLATIONS_ALL ||
            SimulationManager.Instance.CurrentlySelectedConstellation == _constellationName)
        {
            selectedIcon.SetActive(true);
        } else selectedIcon.SetActive(false);
        
    }
}
