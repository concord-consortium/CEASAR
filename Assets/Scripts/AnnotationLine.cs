using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnnotationLine : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private float hoverSize = 1.5f;
    private Vector3 initialScale = Vector3.one;
    private Vector3 hoverScale = Vector3.one;
    private bool isSelected = false;
    private ParticleSystem selectedParticles;
    private bool isDrawing = true; // annotations are only added when drawing is enabled
    private float holdClickDuration = 0;
    private float holdToDeleteTime = 1f;
    private float startParticleSpeed = 0.12f;

    public bool IsSelected {
        get { return isSelected; }
    }

    void Update()
    {
        if (isSelected && Input.GetMouseButton(0))
        {
            holdClickDuration += Time.deltaTime;
            selectedParticles.startSpeed = selectedParticles.startSpeed + (holdClickDuration / 2);
            if (holdClickDuration > holdToDeleteTime)
            {
                isSelected = false;
                holdClickDuration = 0;
                
                HandleDeleteAnnotation();
            }
        } else if (isSelected && holdClickDuration > 0)
        {
            selectedParticles.startSpeed = startParticleSpeed;
            holdClickDuration = 0;
        }
    }
    public void FinishDrawing()
    {
        initialScale = transform.localScale;
        hoverScale = new Vector3(initialScale.x * hoverSize, initialScale.y * hoverSize, initialScale.z);
        selectedParticles = GetComponent<ParticleSystem>();
        SimulationEvents.GetInstance().DrawMode.AddListener(HandleDrawModeToggle);
    }

    private void OnDisable()
    {
        SimulationEvents.GetInstance().DrawMode.RemoveListener(HandleDrawModeToggle);
    }

    public void HandleDrawModeToggle(bool drawModeActive)
    {
        if (isDrawing != drawModeActive)
        {
            isSelected = false;
            transform.localScale = initialScale;
            selectedParticles.Stop();
            GetComponent<Collider>().enabled = !drawModeActive;
            isDrawing = drawModeActive;
        }
    }
    public void Highlight(bool showHighlight)
    {
        if (showHighlight)
        {
            transform.localScale = hoverScale;
        }
        else
        {
            transform.localScale = initialScale;
        }
    }

    public void ToggleSelectAnnotation()
    {
        isSelected = !isSelected;
        if (isSelected) selectedParticles.Play();
        else selectedParticles.Stop();
    }

    public void HandleDeleteAnnotation()
    {
        SimulationEvents.GetInstance().AnnotationDeleted.Invoke(this.name);
        Destroy(this.gameObject);
    }
    
    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData)
    {
       /* if (!isDrawing)
        {
            ToggleSelectAnnotation();
        }*/
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDrawing)
        {
            Highlight(true);
            isSelected = true;
            selectedParticles.Play();
           
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDrawing)
        {
            Highlight(false);
            isSelected = false;
            selectedParticles.Stop();
        }
    }

    #endregion
}
