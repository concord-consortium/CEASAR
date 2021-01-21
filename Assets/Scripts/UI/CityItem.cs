using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityItem : MonoBehaviour
{
    [SerializeField] private GameObject selectedIcon;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text cityNameLabel;
    private string _cityName;
    private bool _isSelected = false;
    
    public void Init(string cityName, bool selected)
    {
        gameObject.name = cityName;
        _cityName = cityName;
        cityNameLabel.SetText(cityName);
        selectButton.onClick.AddListener(() => selectCity(_cityName));
        _isSelected = selected;
        setSelectedStatus();
        SimulationEvents.Instance.PushPinUpdated.AddListener((p, lookDirection) => pinUpdated(p));
    }

    private void selectCity(string cityName)
    {
        bool shouldSelectThisCity = cityName == _cityName;
        if (shouldSelectThisCity && !_isSelected)
        {
            // only change selection and invoke event if this city is the new city to select
            SimulationEvents.Instance.LocationSelected.Invoke(_cityName);
        }

        _isSelected = shouldSelectThisCity;
        setSelectedStatus();
    }

    private void pinUpdated(Pushpin p)
    {
        if (!_isSelected && p.LocationName == _cityName)
        {
            _isSelected = true;
            setSelectedStatus();
        }
        else
        {
            _isSelected = false;
            setSelectedStatus();
        }
    }
    private void setSelectedStatus()
    {
        selectedIcon.SetActive(_isSelected);
    }
}
