using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pushpin : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    public float latitude = 0;
    public float longitude = 0;
    public DateTime selectedDateTime;
    
    private Material defaultMaterial;
    public Material highlightPinMaterial;

    private void Start()
    {
        defaultMaterial = GetComponent<Renderer>().material;
    }

    public void HandleSelectPin()
    {
        SimulationEvents.GetInstance().PushPinSelected.Invoke(new Vector2(latitude, longitude), selectedDateTime);
        SimulationEvents.GetInstance().LocationChanged.Invoke(new Vector2(latitude, longitude), SimulationConstants.CUSTOM_LOCATION);
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
