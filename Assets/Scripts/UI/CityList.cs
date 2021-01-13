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
    [SerializeField] private RectTransform spawnPoint;
    public void InitCityNames(List<string> cityNames, string currentCity)
    {
        contentContainer.sizeDelta = new Vector2(0, cityNames.Count * 20);     
        if (allCityNames == null || allCityNames.Count == 0)
        {
            string[] cities = cityNames.ToArray();
            for (int i = 0; i < cities.Length; i++)
            {
                GameObject cityObject = Instantiate(cityTextObject);
                cityObject.transform.SetParent(contentContainer);
                cityObject.transform.localScale = Vector3.one;
                Vector3 p = cityObject.transform.localPosition;
                cityObject.transform.localPosition = new Vector3(p.x, p.y, -0.1f);
                
                cityObject.GetComponent<TMP_Text>().text = cities[i];
                cityObject.name = cities[i];
            }

            // cache the list of names
            allCityNames = cityNames;
        }
        
    }

}
