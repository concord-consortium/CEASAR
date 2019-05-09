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
        get { return allStars;}
        private set { allStars = value;}
    }

    [SerializeField]
    private List<City> allCities;
    public List<City> AllCities
    {
        get { return allCities;}
        private set { allCities = value;}
    }


    // Start is called before the first frame update
    void Start()
    {
        if (starData != null){
            allStars = DataImport.ImportStarData(starData.text);
            Debug.Log(allStars.Count + " stars imported");
        }
        if(cityData != null){
            allCities = DataImport.ImportCityData(cityData.text);
            Debug.Log(allCities.Count + " cities imported");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
