﻿using System.Collections.Generic;
using System.Text;
using UnityEngine;

// This class is the manager Singleton, and contains specific references to application-level objects
public class SimulationManager
{
    // can't use constructor, guaranteed singleton
    protected SimulationManager() { }
    private static SimulationManager instance;

    public static SimulationManager GetInstance()
    {
        return instance ?? (instance = new SimulationManager());
    }

    // A decent random generator
    public System.Random rng = new System.Random();

    // List of scenes in project for current build target
    private readonly string[] _desktopScenes = new string[4] { "LoadSim", "Stars", "Horizon", "EarthInteraction" };
    private readonly string[] _ARScenes = new string[4] { "LoadSim", "Stars", "Horizon", "EarthInteraction" };
    private readonly string[] _VRScenes = new string[4] { "LoadSim", "Stars", "Horizon", "EarthInteraction" };

    public string[] Scenes {
        get {
#if UNITY_STANDALONE
            return this._desktopScenes;
#elif UNITY_IOS
            return this._ARScenes;
#elif UNITY_ANDROID
            return this._VRScenes;
#else
            // Catch-all default
            return this._desktopScenes;
#endif
        }
    }


    public GameObject NetworkControllerObject;
    public NetworkConnection NetworkStatus = NetworkConnection.None; 
    private GameObject celestialSphereObject;

    public GameObject CelestialSphereObject
    {
        get { return celestialSphereObject; }
        set
        {
            celestialSphereObject = value;
            DataControllerComponent = celestialSphereObject.GetComponent<DataController>();
            MarkersControllerComponent = celestialSphereObject.GetComponentInChildren<MarkersController>();
            ConstellationsControllerComponent = celestialSphereObject.GetComponentInChildren<ConstellationsController>();
        }
    }

    public DataController DataControllerComponent { get; private set; }
    public MarkersController MarkersControllerComponent { get; private set; }
    public ConstellationsController ConstellationsControllerComponent { get; private set; }

    public bool IsReady = false;

    public string[] AnimalNames;
    public List<string> ColorNames = new List<string>();
    public List<Color> ColorValues = new List<Color>();

    private Color localPlayerColor = Color.white;
    public Color LocalPlayerColor {
        get { return localPlayerColor; }
    }

    // initial setup scale
    public readonly float InitialRadius = 100;
    public float CurrentScaleFactor(float sceneRadius)
    {
        return sceneRadius / InitialRadius;
    }
    // Movement synchronization throttled for Heroku/Mongo
    public float MovementSendInterval = 1.0f;
    
    // Server Address!
    public string LocalNetworkServer = "ws://localhost:2567";
    public string DevNetworkServer = "ws://ceasar-serve-166903181-luo6i9u.herokuapp.com/";
    public string ProductionNetworkServer = "ws://ceasar-server-staging.herokuapp.com/";

    public StarComponent CurrentlySelectedStar;

    private string localUsername = "";
    // Random color (capitalized), random animal (capitalized), random number
    public void GenerateUsername()
    {
        int colorIndex = rng.Next(ColorNames.Count - 1);
        int animalIndex = rng.Next(AnimalNames.Length - 1);
        string randomNumber = rng.Next(999).ToString();
        localUsername = ColorNames[colorIndex].FirstCharToUpper() + AnimalNames[animalIndex].FirstCharToUpper() + randomNumber;
        localPlayerColor = ColorValues[colorIndex];
    }

    // We can find out the color value from the username
    public Color GetColorForUsername(string name)
    {
        StringBuilder sb = new StringBuilder();
        bool found = false;
        int i = 0;
        while (!found && i < name.Length)
        {
            if (char.IsUpper(name[i]) && i > 1)
            {
                found = true;
            }
            else
            {
                sb.Append(name[i]);
                i++;
            }
        }

        string colorName = sb.ToString();
        // Don't forget to lowercase the name!
        string color = colorName.ToLower();
        if (ColorNames.Contains(color))
        {
            return ColorValues[ColorNames.IndexOf(color)];
        }
        else
        {
            Debug.Log("Color not found for " + color + " as part of " + name);
            return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
        }
    }

    public string LocalUsername {
        get { return localUsername; }
        set { 
            localUsername = value;
            localPlayerColor = GetColorForUsername(localUsername);
        }
    }

    public float GetRelativeMagnitude(float starMagnitude)
    {
        //float min = DataControllerComponent.minMag;
        float max = DataControllerComponent.maxMag + Mathf.Abs(DataControllerComponent.minMag);
        return max - (starMagnitude + Mathf.Abs(DataControllerComponent.minMag));
    }
}