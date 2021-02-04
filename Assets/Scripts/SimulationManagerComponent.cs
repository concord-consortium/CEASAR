using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;

// This component is a MonoBehaviour so we can add references to prefabs and scene stuff in the inspector
public class SimulationManagerComponent : MonoBehaviour
{
    // A local reference to the Singleton manager
    private SimulationManager manager;

    [SerializeField]
    private GameObject networkControllerPrefab;
    [SerializeField]
    private GameObject interactionControllerPrefab;
    [SerializeField]
    private GameObject celestialSpherePrefab;

    [SerializeField] private GameObject mainUIPrefab;
    [SerializeField] private GameObject infoPanelPrefab;
    [SerializeField]
    private Material starMaterial;
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
    [SerializeField]
    float markerLineWidth = .1f;
    [SerializeField]
    float markerScale = 1f;

    // Settings for constellations
    [SerializeField]
    private float lineWidth = .035f;

    [SerializeField] private StarComponent starPrefab;

    [SerializeField]
    private SceneLoader sceneLoaderPrefab;
    private SceneLoader sceneLoader;

    private void Awake()
    {
        manager = SimulationManager.Instance;
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
            CCDebug.Log("Manager ready");
        }
        else
        {
            CCDebug.Log("Manager configuration already set");
        }
        CCDebug.Log("Applying scene settings");
        Setup();
    }

    void Setup()
    {
        SceneLoader existingLoader = FindObjectOfType<SceneLoader>();
        if (existingLoader != null)
        {
            sceneLoader = existingLoader;
        } else
        {
            sceneLoader = Instantiate(sceneLoaderPrefab);
        }
        
        if (manager.NetworkControllerComponent == null)
        {
            if (FindObjectOfType<NetworkController>() != null)
            {
                manager.NetworkControllerComponent = FindObjectOfType<NetworkController>();
            }
            else
            {
                CCDebug.Log("Creating new network object");
                manager.NetworkControllerComponent = Instantiate(networkControllerPrefab).GetComponent<NetworkController>();
            }
        }
        if (manager.InteractionControllerObject == null)
        {
            if (FindObjectOfType<InteractionController>() != null)
            {
                manager.InteractionControllerObject = FindObjectOfType<InteractionController>().gameObject;
            }
            else
            {
                CCDebug.Log("Creating new Interaction controller object");
                manager.InteractionControllerObject = Instantiate(interactionControllerPrefab);
            }
        }
        if (manager.CelestialSphereObject == null)
        {
            CCDebug.Log("Creating new Celestial Sphere");
            manager.CelestialSphereObject = Instantiate(celestialSpherePrefab);
            if (this.starPrefab != null)
            {
                manager.DataControllerComponent.starPrefab = this.starPrefab;
            }

            if (!starMaterial) starMaterial = starPrefab.GetComponent<Renderer>().material;
            // When creating, set parameters before Init
            manager.DataControllerComponent.SetSceneParameters(maxStars, magnitudeScale, magnitudeThreshold,
                 showHorizonView, simulationTimeScale, radius, colorByConstellation, showConstellationConnections, starMaterial);

            // Create all the stars
            manager.DataControllerComponent.Init();

            // Set up the markers with a reference to the dataController for parameters like current radius
            manager.MarkersControllerComponent.Init();
            manager.MarkersControllerComponent.SetSceneParameters(markerLineWidth, markerScale, showMarkers, showPoleLine, showEquator);

            // Show/hide Marker items toggles visibility of existing objects
            manager.ConstellationsControllerComponent.SetSceneParameters(lineWidth, showConstellationConnections);

            AnnotationTool annotationTool = FindObjectOfType<AnnotationTool>();
            annotationTool.Init();

            manager.DataControllerComponent.UpdateOnSceneLoad();
        }
        else
        {
            // Applying scene-specific settings
            if (manager.DataControllerComponent)
            {
                if (!starMaterial) starMaterial = starPrefab.GetComponent<Renderer>().material;
                manager.DataControllerComponent.SetSceneParameters(maxStars, magnitudeScale, magnitudeThreshold,
                  showHorizonView, simulationTimeScale, radius, colorByConstellation, showConstellationConnections, starMaterial);
                manager.DataControllerComponent.UpdateOnSceneLoad();
            }
            if (manager.MarkersControllerComponent)
            {
                manager.MarkersControllerComponent.SetSceneParameters(markerLineWidth, markerScale, showMarkers, showPoleLine, showEquator);
            }
            if (manager.ConstellationsControllerComponent)
            {
                manager.ConstellationsControllerComponent.SetSceneParameters(lineWidth, showConstellationConnections);
            }
        }
        if (!manager.MainMenu) 
        {
            Instantiate(mainUIPrefab);
            manager.NetworkControllerComponent.Setup();
        } 
       
        if (!manager.InfoPanel) 
        {
            Instantiate(infoPanelPrefab);
        } 
        sceneLoader.SetupCameras();
        
    }

    public void HandleEarthInteraction(InputEventData inputEvent)
    {
        Debug.LogWarning(inputEvent);
        if (manager.InteractionControllerObject)
        {
            manager.InteractionControllerObject.GetComponent<InteractionController>().SetEarthLocationPin(inputEvent.selectedObject.transform.position);
        }
    }

}