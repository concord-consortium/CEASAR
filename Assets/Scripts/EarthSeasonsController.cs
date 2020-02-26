using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSeasonsController : MonoBehaviour
{
    public Texture2D[] seasons;

    private SimulationManager manager;
    private int _month = -1;

    private float axisTilt = 23.5f;
    private DateTime lastTime;

    public GameObject sunlight;
    private void Start()
    {
        manager = SimulationManager.GetInstance();
        // Reset the local month each time we load so that changes in the central time force a texture update
        _month = -1;
        if (sunlight == null) sunlight = GameObject.Find("Sun");
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        int currentMonth = manager.CurrentSimulationTime.Month - 1;
        if (_month != currentMonth)
        {
            // Change Earth texture to the matching texture for the current month
            GetComponent<Renderer>().material.SetTexture("_MainTex", seasons[currentMonth]);
            _month = currentMonth;
        }
        if (sunlight && lastTime != manager.CurrentSimulationTime)
        {
            lastTime = manager.CurrentSimulationTime;
            
            float xRotation = Mathf.Sin((manager.CurrentSimulationTime.DayOfYear - 90) * Mathf.Deg2Rad) * axisTilt;
            float yRotation = sunlight.transform.rotation.eulerAngles.y;
            sunlight.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}
