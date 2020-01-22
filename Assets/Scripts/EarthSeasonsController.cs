using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSeasonsController : MonoBehaviour
{
    public Texture2D[] seasons;

    private SimulationManager manager;
    private int _month = -1;

    private void Start()
    {
        manager = SimulationManager.GetInstance();
        // Reset the local month each time we load so that changes in the central time force a texture update
        _month = -1;
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        int currentMonth = manager.CurrentSimulationTime.Month - 1;
        if (_month != currentMonth)
        {
            Debug.Log(manager.CurrentSimulationTime);
            // Change Earth texture to the matching texture for the current month
            GetComponent<Renderer>().material.SetTexture("_MainTex", seasons[currentMonth]);
            _month = currentMonth;
        }
    }
}
