using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PushpinComponent : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{

    public string owner;
    
    private Material defaultMaterial;
    public Material highlightPinMaterial;

    private SimulationManager manager { get {return SimulationManager.GetInstance();}}
    private void Start()
    {
        owner = "";
        defaultMaterial = GetComponent<Renderer>().material;
    }

    public void HandleSelectPinObject()
    {
        bool isLocalPlayer = owner == manager.LocalUsername;
        if (isLocalPlayer)
        {
            SimulationEvents.GetInstance().PushPinSelected.Invoke(manager.LocalPlayerPin);
        }
        else
        {
            Pushpin pin = manager.GetRemotePlayer(owner).Pin;
            SimulationEvents.GetInstance().PushPinSelected.Invoke(pin);
        }

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
        HandleSelectPinObject();
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
