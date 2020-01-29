﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushpinComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Pushpin pin;
    
    private Material defaultMaterial;
    public Material highlightPinMaterial;

    private void Start()
    {
        defaultMaterial = GetComponent<Renderer>().material;
        if (SimulationManager.GetInstance().UserHasSetLocation)
        {
            pin = SimulationManager.GetInstance().LocalUserPin;
        }
        else
        {
            pin = new Pushpin
            {
                Location = new LatLng {Latitude = 0, Longitude = 0},
                SelectedDateTime = SimulationManager.GetInstance().CurrentSimulationTime
            };
        }
    }

    public void HandleSelectPin()
    {
        LatLng latlng = pin.Location;
        SimulationEvents.GetInstance().PushPinSelected.Invoke(latlng, pin.SelectedDateTime);
        SimulationEvents.GetInstance().LocationChanged.Invoke(latlng, SimulationConstants.CUSTOM_LOCATION);
    }

    public void HighlightPin(bool highlight)
    {
        if (highlight)
        {
            GetComponent<Renderer>().material = highlightPinMaterial;
            highlightPinMaterial.color = defaultMaterial.color;
        }
        else
        {
            GetComponent<Renderer>().material = defaultMaterial;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        HandleSelectPin();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        HighlightPin(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        HighlightPin(false);
    }
}