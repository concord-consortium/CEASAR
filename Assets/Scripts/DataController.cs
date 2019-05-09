using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataController : MonoBehaviour
{
    public TextAsset starData;
    public TextAsset cityData;

    [SerializeField]
    private float radius = 50;
    [SerializeField]
    private float magnitudeScale = 0.5f;

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
            // get magnitudes and normalize between 1 and 5 to scale stars
            var minMag = allStars.Min(s => s.Mag);
            var maxMag = allStars.Max(s => s.Mag);
            var constellations = new List<string>(allStars.GroupBy(s => s.Constellation).Select(s => s.First().Constellation));  //new Dictionary<string, List<Star>>();
            Debug.Log(minMag + " " + maxMag + " constellations:" + constellations.Count);

            foreach (string constellation in constellations)
            {
                List<Star> starsInConstellation = allStars.Where(s => s.Constellation == constellation).ToList();
                GameObject constellationContainer = new GameObject();
                constellationContainer.name = constellation;
                Color constellationColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.8f, 1f);

                foreach (Star dataStar in starsInConstellation)
                {
                    GameObject starObject = Instantiate(dataStarPrefab, this.transform.position, Quaternion.identity);
                    StarComponent newStar = starObject.GetComponent<StarComponent>();
                    newStar.starData = dataStar;
                    starObject.name = dataStar.Constellation;
                    starObject.transform.position = newStar.starData.CalculateEquitorialPosition(radius);

                    // rescale for magnitude
                    var magScaleValue = ((dataStar.Mag * -1) + maxMag + 1) * magnitudeScale;
                    Vector3 magScale = starObject.transform.localScale * magScaleValue;
                    starObject.transform.localScale = magScale;
                    starObject.transform.LookAt(this.transform);

                    // color by constellation
                    starObject.GetComponent<Renderer>().material.SetColor("_BaseColor", constellationColor);

                    // group by constellation
                    starObject.transform.parent = constellationContainer.transform;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
