using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;

    [SerializeField]
    private List<Star> allStars;
    public List<Star> AllStars
    {
        get { return allStars; }
        private set { allStars = value; }
    }

    [SerializeField]
    private List<City> allCities;
    public List<City> AllCities
    {
        get { return allCities; }
        private set { allCities = value; }
    }

    public GameObject dataStarPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (starData != null)
        {
            allStars = DataImport.ImportStarData(starData.text);
            Debug.Log(allStars.Count + " stars imported");
        }
        if (cityData != null)
        {
            allCities = DataImport.ImportCityData(cityData.text);
            Debug.Log(allCities.Count + " cities imported");
        }

        if (dataStarPrefab != null && allStars != null && allStars.Count > 0)
        {
            foreach (Star dataStar in allStars)
            {
                GameObject starObject = Instantiate(dataStarPrefab, Random.onUnitSphere * 50, Quaternion.identity);
                starObject.transform.LookAt(transform);
                StarComponent newStar = starObject.GetComponent<StarComponent>();
                newStar.starData = dataStar;
                starObject.name = dataStar.Constellation;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
