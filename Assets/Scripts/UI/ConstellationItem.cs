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
        selectButton.onClick.AddListener(() => selectConstellation(_constellationName));
        _isSelected = selected;
        
        setSelectedStatus();
        SimulationEvents.Instance.StarSelected.AddListener((star) => onStarSelected(star));
    }

    private void selectConstellation(string constellationName)
    {
        bool shouldSelectThisConstellation = constellationName == _constellationName;
        if (shouldSelectThisConstellation && !_isSelected)
        {
            // only change selection and invoke event if this city is the new city to select
            constellationsController.SelectConstellationByName(constellationName);
            _isSelected = true;
        }

        _isSelected = shouldSelectThisConstellation;
        setSelectedStatus();
    }
    
    private void setSelectedStatus()
    {
        selectedIcon.SetActive(_isSelected);
    }

    private void onStarSelected(Star star)
    {
        if (star == null)
        {
            if (SimulationManager.Instance.CurrentlySelectedConstellation == "all")
            {
                selectedIcon.SetActive(true);
            } else selectedIcon.SetActive(false);
        }
        else
        {
            selectedIcon.SetActive(star.Constellation == _constellationName);
        }
    }
}
