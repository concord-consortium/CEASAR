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
    public bool showHorizonView = false;

    public GameObject starPrefab;
    public GameObject allConstellations;

    private float simulationHour = 0f;
    public float simulationTimeScale = 0.001f;

    public List<string> cities;
    private City currentCity;
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
            cities = allCities.Select(c => c.Name).ToList();
            currentCity = allCities.Where(c => c.Name == "Boston").FirstOrDefault();
        }

        if (starPrefab != null && allStars != null && allStars.Count > 0)
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
                constellationContainer.transform.parent = allConstellations.transform;

                Color constellationColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);

                foreach (Star dataStar in starsInConstellation)
                {
                    GameObject starObject = Instantiate(starPrefab, this.transform.position, Quaternion.identity);
                    StarComponent newStar = starObject.GetComponent<StarComponent>();
                    newStar.starData = dataStar;
                    starObject.name = dataStar.Constellation;
                    if (showHorizonView)
                    {
                        starObject.transform.position = newStar.starData.CalculateHorizonPosition(radius, 0, 0);
                    }
                    else
                    {
                        starObject.transform.position = newStar.starData.CalculateEquitorialPosition(radius);
                    }

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

    void Update()
    {  
        if (showHorizonView){
            simulationHour += simulationTimeScale;
            foreach (StarComponent starObject in GameObject.FindObjectsOfType<StarComponent>()){
                starObject.gameObject.transform.position = starObject.starData.CalculateHorizonPosition(radius, System.DateTime.Now.Hour + simulationHour, currentCity.Lat);
                starObject.transform.LookAt(this.transform);
            }
        }
    }
}
