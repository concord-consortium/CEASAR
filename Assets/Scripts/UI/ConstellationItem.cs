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
    
    public void Init(string constellationName, bool selected)
    {
        constellationsController = FindObjectOfType<ConstellationsController>();
        
        gameObject.name = constellationName;
        _constellationName = constellationName;
        constellationLabel.SetText(constellationName);
        selectButton.onClick.AddListener(() => selectThisConstellation());
        _isSelected = selected;
        selectedIcon.SetActive(_isSelected);
        
        SimulationEvents.Instance.StarSelected.AddListener((star) => onStarSelected());
        SimulationEvents.Instance.ConstellationSelected.AddListener((constellationSelectedName) => onSelectConstellation(constellationSelectedName));
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
