using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

// This component is a MonoBehaviour so we can add references to prefabs and scene stuff in the inspector
public class SimulationManagerComponent : MonoBehaviour
{
    // A local reference to the Singleton manager
    private SimulationManager manager;

    [SerializeField]
    private GameObject networkControllerPrefab;
    [SerializeField]
    private GameObject celestialSpherePrefab;

    // Celestial Sphere scene-specific settings
    [SerializeField]
    private int maxStars = 10000;
    [SerializeField]
    private float radius = 50;
    [SerializeField]
    private float magnitudeScale = 0.5f;
    [SerializeField]
    private float magnitudeThreshold = 4.5f;
    [SerializeField]
    private float minMag;
    [SerializeField]
    private float maxMag;
    [SerializeField]
    private bool showHorizonView = false;
    [SerializeField]
    private float simulationTimeScale = 10f;
    [SerializeField]
    private bool colorByConstellation = false;
    [SerializeField]
    private bool showConstellationConnections = false;

    // Settings for markers
    [SerializeField]
    private bool showMarkers;
    [SerializeField]
    private bool showPoleLine;
    [SerializeField]
    private bool showEquator;

    private void Awake()
    {
        manager = SimulationManager.GetInstance();
        if (!manager.IsReady)
        {
            TextAsset colorList = Resources.Load("colors") as TextAsset;
            TextAsset animalList = Resources.Load("animals") as TextAsset;
            char[] lineDelim = new char[] { '\r', '\n' };
            string[] colorsFull = colorList.text.Split(lineDelim, StringSplitOptions.RemoveEmptyEntries);
            List<string> colors = new List<string>();
            List<Color> colorValues = new List<Color>();
            foreach (string c in colorsFull)
            {
                colors.Add(c.Split(':')[0]);
                Color color;
                ColorUtility.TryParseHtmlString(c.Split(':')[1], out color);
                colorValues.Add(color);
            }
            manager.ColorNames = colors;
            manager.ColorValues = colorValues;
            manager.AnimalNames = animalList.text.Split(lineDelim, StringSplitOptions.RemoveEmptyEntries);
            manager.IsReady = true;
            Debug.Log("Manager ready");
        }
        else
        {
            Debug.Log("Manager configuration already set");
        }

    }
    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    Setup();
    //}
    //private void OnApplicationQuit()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}
    private void Start()
    {
        Debug.Log("Applying scene settings");
        Setup();
    }

    void Setup()
    {
        if (manager.NetworkControllerObject == null)
        {
            Debug.Log("Creating new network object");
            manager.NetworkControllerObject = Instantiate(networkControllerPrefab);
        }
        if (manager.CelestialSphereObject == null)
        {
            Debug.Log("Creating new Celestial Sphere");
            manager.CelestialSphereObject = Instantiate(celestialSpherePrefab);

            // When creating, set parameters before Init
            manager.DataControllerComponent.SetSceneParameters(maxStars, magnitudeScale, magnitudeThreshold, minMag, maxMag,
                 showHorizonView, simulationTimeScale, radius, colorByConstellation, showConstellationConnections);

            manager.DataControllerComponent.Init();

            manager.MarkersControllerComponent.Init();
            // Show/hide Marker items toggles visibility of existing objects
            manager.MarkersControllerComponent.ShowMarkers(showMarkers, showPoleLine, showEquator);
            manager.ConstellationsControllerComponent.ShowAllConstellations(showConstellationConnections);


            manager.DataControllerComponent.UpdateOnSceneLoad();
        }
        else
        {
            // Applying scene-specific settings
            if (manager.DataControllerComponent)
            {
                manager.DataControllerComponent.SetSceneParameters(maxStars, magnitudeScale, magnitudeThreshold, minMag, maxMag,
                  showHorizonView, simulationTimeScale, radius, colorByConstellation, showConstellationConnections);
                manager.DataControllerComponent.UpdateOnSceneLoad();
            }
            if (manager.MarkersControllerComponent)
            {
                manager.MarkersControllerComponent.ShowMarkers(showMarkers, showPoleLine, showEquator);
            }
            if (manager.ConstellationsControllerComponent)
            {
                manager.ConstellationsControllerComponent.ShowAllConstellations(showConstellationConnections);
            }
        }
    }

}