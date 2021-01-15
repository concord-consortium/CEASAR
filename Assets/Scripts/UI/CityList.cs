using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CityList : MonoBehaviour
{
    private List<string> allCityNames;
    [SerializeField] private GameObject cityTextObject;
    [SerializeField] private RectTransform contentContainer;
    public void InitCityNames(List<string> cityNames, string currentCity)
    {
        if (allCityNames == null || allCityNames.Count == 0)
        {
            string[] cities = cityNames.ToArray();
            for (int i = 0; i < cities.Length; i++)
            {
                GameObject cityObject = Instantiate(cityTextObject);
                cityObject.transform.SetParent(contentContainer);
                cityObject.transform.localScale = Vector3.one;
                bool isSelected = SimulationManager.Instance.CurrentLocationName == cities[i];
                cityObject.GetComponent<CityItem>().Init(cities[i], isSelected);
            }

            // cache the list of names
            allCityNames = cityNames;
        }
        
    }

}
