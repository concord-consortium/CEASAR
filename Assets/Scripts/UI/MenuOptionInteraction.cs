using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// A non-VR set of event handlers for regular desktop interaction with the menu
/// </summary>
public class MenuOptionInteraction : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private MenuOption _menuOption;
    public void Setup(MenuOption menuOption)
    {
        _menuOption = menuOption;
    }
   
    public void OnPointerEnter(PointerEventData eventData)
    {
        _menuOption.Hover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _menuOption.Hover(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _menuOption.Click(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _menuOption.Click(false);
    }


}
