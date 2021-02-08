using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CityList : MonoBehaviour
{
    private List<string> allCityNames;
    [SerializeField] private GameObject cityTextObject;
    [SerializeField] private RectTransform contentContainer;
    public bool HasCompletedSetup = false;
    public IEnumerator InitCityNames(List<string> cityNames, string currentCity, bool forceUpdate)
    {
        yield return new WaitForEndOfFrame();
        CityItem[] _existingCityItems = FindObjectsOfType<CityItem>();
        if (_existingCityItems.Length == 0)
        {
            if (allCityNames == null || allCityNames.Count == 0)
            {
                if (forceUpdate)
                {
                    // Keeping the option to regenerate the city list in case our list changes
                    // in future updates. Ideally, we'll run this in the editor and use a pre-populated prefab
                    // to speed things up at runtime. This also helps when moving to VR since all child elements are
                    // positioned together when switching to world-space canvas
                    string[] cities = cityNames.ToArray();
                    if (_existingCityItems.Length == 0)
                    {
                        for (int i = 0; i < cities.Length; i++)
                        {
                            Debug.Log(cities[i] + " Creating!");
                            GameObject cityObject = Instantiate(cityTextObject);
                            cityObject.transform.SetParent(contentContainer);
                            cityObject.transform.localScale = Vector3.one;
                            bool isSelected = SimulationManager.Instance.CurrentLocationName == cities[i];
                            cityObject.GetComponent<CityItem>().Init(cities[i], isSelected);
                        }
                    }
                }

                // cache the list of names
                allCityNames = cityNames;
            }
        }
        HasCompletedSetup = true;
    }
}
