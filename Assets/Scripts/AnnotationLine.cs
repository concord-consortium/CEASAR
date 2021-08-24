using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnnotationLine : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
{
    private float hoverSize = 1.5f;
    private bool isSelected = false;
    private bool isDrawing = true; // annotations are only added when drawing is enabled
    private float holdClickDuration = 0;
    private float holdToDeleteTime = 1f;
    private AnnotationTool _annotationTool;
    private Color initialColor;
    private float initialLineRendererWidth = 0;
    private float hoverLineRendererWidth = 0;
    public GameObject StartPoint;
    public Vector3 StartPos;
    public Vector3 EndPos;

    public bool IsSelected {
        get { return isSelected; }
        set { isSelected = value; }
    }

    void Update()
    {
        if (isSelected)
        {
            HoldToDeleteAnnotation(Input.GetMouseButton(0));
        }
    }
    public void StartDrawing(Vector3 startPos)
    {
        if (StartPoint) StartPoint.transform.position = startPos;
    }
    public void RemoveStartPoint()
    {
        Destroy(StartPoint);
    }

    public void FinishDrawing()
    {
        initialLineRendererWidth = this.GetComponent<LineRenderer>().startWidth;
        hoverLineRendererWidth = initialLineRendererWidth * hoverSize;
        initialColor = this.GetComponent<Renderer>().material.color;
        if (_annotationTool == null) _annotationTool = FindObjectOfType<AnnotationTool>();
        SimulationEvents.Instance.DrawMode.AddListener(HandleDrawModeToggle);
    }

    private void OnDisable()
    {
        SimulationEvents.Instance.DrawMode.RemoveListener(HandleDrawModeToggle);
    }

    public void HandleDrawModeToggle(bool drawModeActive)
    {
        if (isDrawing != drawModeActive)
        {
            isSelected = false;
            this.GetComponent<LineRenderer>().startWidth = initialLineRendererWidth;
            this.GetComponent<LineRenderer>().endWidth = initialLineRendererWidth;
            this.GetComponent<LineRenderer>().material.color = initialColor;
            GetComponent<Collider>().enabled = !drawModeActive;
            isDrawing = drawModeActive;
        }
    }
    public void Highlight(bool showHighlight)
    {
        if (_annotationTool.IsMyAnnotation(this.gameObject))
        {
            if (showHighlight)
            {
                this.GetComponent<LineRenderer>().startWidth = hoverLineRendererWidth;
                this.GetComponent<LineRenderer>().endWidth = hoverLineRendererWidth;
                isSelected = true;
            }
            else
            {
                this.GetComponent<LineRenderer>().startWidth = initialLineRendererWidth;
                this.GetComponent<LineRenderer>().endWidth = initialLineRendererWidth;
                this.GetComponent<LineRenderer>().material.color = initialColor;
                isSelected = false;
            }
        }
    }

    public void HoldToDeleteAnnotation(bool holdToDelete)
    {
        if (isSelected)
        {
            if (holdToDelete)
            {
                holdClickDuration += Time.deltaTime;
                if (holdClickDuration > holdToDeleteTime)
                {
                    isSelected = false;
                    holdClickDuration = 0;

                    HandleDeleteAnnotation();
                }
                Color currentColor = this.GetComponent<Renderer>().material.color;
                float r = Mathf.Max(0, currentColor.r + Time.deltaTime);
                float g = Mathf.Max(0, currentColor.g + Time.deltaTime);
                float b = Mathf.Max(0, currentColor.b + Time.deltaTime);
                Color fadeColor = new Color(r, g, b, 1f);
                this.GetComponent<LineRenderer>().material.color = fadeColor;
            }
            else if (holdClickDuration > 0)
            {
                holdClickDuration = 0;
               this.GetComponent<LineRenderer>().material.color = initialColor;
            }
        }
    }

    public void HandleDeleteAnnotation()
    {
        SimulationEvents.Instance.AnnotationDeleted.Invoke(this.name);
        Destroy(this.gameObject);
    }

    #region MouseEvent Handling
    public void OnPointerDown(PointerEventData eventData) { }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDrawing)
        {
            Highlight(true);
            isSelected = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDrawing)
        {
            Highlight(false);
            isSelected = false;
        }
    }

    #endregion
}
