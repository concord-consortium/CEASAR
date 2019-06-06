using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This component is a MonoBehaviour so we can add references to prefabs and scene stuff in the inspector
public class SimulationManagerComponent : MonoBehaviour
{
    // A local reference to the Singleton manager
    private SimulationManager manager;

    [SerializeField]
    private GameObject networkControllerPrefab;

    private void Awake()
    {
        manager = SimulationManager.GetInstance();

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
    }
    private void Start()
    {
        if (manager.NetworkControllerObject == null)
        {
            Debug.Log("Creating new network object");
            manager.NetworkControllerObject = Instantiate(networkControllerPrefab);
        }
    }
}