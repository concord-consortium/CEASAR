using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

using UnityEngine.EventSystems;
public class MenuOption : MonoBehaviour
{
    public string DisplayText;
    public string TooltipText;
    public AudioClip ClickSound;
    public Sprite ButtonIcon;
    public string IconText;
    public UnityEvent OnClick;
    public bool holdButtonEnabled;

    private bool isHeld = false;
    
    [SerializeField] private GameObject buttonText;
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private GameObject textIconCharacter;
    [SerializeField] private GameObject spriteIcon;
    [SerializeField] private GameObject interactionHandlerObject;
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(DisplayText))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            if (buttonText)
            {
                buttonText.GetComponent<TMP_Text>().text = DisplayText;
            }
            
            if (buttonAnimator == null)
            {
                buttonAnimator = transform.parent.gameObject.GetComponent<Animator>();
            }

            if (tooltipObject)
            {
                tooltipObject.GetComponent<TMP_Text>().text = TooltipText;
            }

            if (ButtonIcon != null && spriteIcon != null)
            {
                spriteIcon.SetActive(true);
                spriteIcon.GetComponent<SpriteRenderer>().sprite = ButtonIcon;
                textIconCharacter.SetActive(false);
            }
            else
            {
                textIconCharacter.SetActive(true);
                textIconCharacter.GetComponent<TMP_Text>().text = IconText;
                spriteIcon.SetActive(false);
            }

            // Interaction handlers may change between platforms. Each handler script needs a public
            // method called Setup that can be invoked here. Each handler will call back here for 
            // generic handling of states, without this script needing to know if we have a pointer,
            // or a hand, etc.
            interactionHandlerObject.SendMessage("Setup", this);
        }
    }

    public void Hover(bool isHovering)
    {
        if (isHovering)
        {
            buttonAnimator.SetBool("hover", true);
            if (tooltipObject) tooltipObject.SetActive(true);   
        }
        else
        {
            buttonAnimator.SetBool("hover", false);
            if (tooltipObject) tooltipObject.SetActive(false);
        }
    }
    
    public void Click(bool isClicking)
    {
        if (isClicking)
        {
            buttonAnimator.SetBool("click", true);
            if (holdButtonEnabled) isHeld = true;
        }
        else
        {
            if (!isHeld)
            {
                playButtonSound();
            }
            isHeld = false;
            buttonAnimator.SetBool("click", false);
            
            OnClick.Invoke();
        }
    }
    public void TestButton()
    {
        Debug.Log(DisplayText + " " + TooltipText);
    }

    private void playButtonSound()
    {
        if (ClickSound)
        {
            GetComponent<AudioSource>().PlayOneShot(ClickSound);
        }
    }
    private void Update()
    {
        if (isHeld)
        {
            OnClick.Invoke();
        }
    }
}
