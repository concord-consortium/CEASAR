using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOptionInteraction : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public string DisplayText;
    public string TooltipText;
    public AudioClip ClickSound;
    public UnityEvent OnClick;

    [SerializeField]
    private Animator buttonAnimator;
    [SerializeField]
    private GameObject tooltipObject;
    

    private void OnEnable()
    {
        if (buttonAnimator == null)
        {
            buttonAnimator = transform.parent.gameObject.GetComponent<Animator>();
        }

        if (tooltipObject)
        {
            tooltipObject.GetComponent<TMP_Text>().text = TooltipText;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonAnimator.SetBool("hover", true);
        if (tooltipObject) tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonAnimator.SetBool("hover", false);
        
        if (tooltipObject) tooltipObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonAnimator.SetBool("click", true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonAnimator.SetBool("click", false);
        OnClick.Invoke();
    }

    public void TestButton()
    {
        Debug.Log(DisplayText + " " + TooltipText);
        if (ClickSound)
        {
            GetComponent<AudioSource>().PlayOneShot(ClickSound);
        }
    }

}
