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
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        int currentMonth = manager.DataControllerComponent.CurrentSimUniversalTime.Month - 1;
        if (_month != currentMonth)
        {
            Debug.Log(manager.DataControllerComponent.CurrentSimUniversalTime);
            GetComponent<Renderer>().material.SetTexture("_MainTex", seasons[currentMonth]);
            _month = currentMonth;
        }
    }
}
