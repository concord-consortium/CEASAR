using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AnnotationTool : MonoBehaviour
{
    private Vector3 startPointForDrawing = Vector3.zero;
    private Vector3 endPointForDrawing = Vector3.zero;

    public bool singleLines = true;
    
    public GameObject annotationLinePrefab;
    public GameObject annotationLineHighlightPrefab;
    
    public float annotationWidth = 1;
    public float annotationHighlightWidthMultiplier = 1.5f;
    private List<GameObject> annotations;

    private LineRenderer annotationLineRenderer;
    private List<Vector3> annotationLinePoints;
    private void Start()
    {
        annotationLineRenderer = this.GetComponent<LineRenderer>();
        annotations = new List<GameObject>();
        annotationLinePoints = new List<Vector3>();
        this.transform.parent = SimulationManager.GetInstance().CelestialSphereObject.transform;
        SimulationEvents.GetInstance().AnnotationReceived.AddListener(AddAnnotation);
    }

    private void OnDisable()
    {
        SimulationEvents.GetInstance().AnnotationReceived.RemoveListener(AddAnnotation);
    }

    public void Annotate(Vector3 nextPoint)
    {
        if (singleLines && annotationLinePrefab)
        {
            if (startPointForDrawing == Vector3.zero)
            {
                // start
                startPointForDrawing = nextPoint;
                annotations.Add(Instantiate(annotationLinePrefab, startPointForDrawing, Quaternion.identity, this.transform));
                
            }
            else if (endPointForDrawing == Vector3.zero)
            {
                // stretch most recent annotation to the end point
                endPointForDrawing = nextPoint;
                
                // Broadcast adding an annotation
                SimulationEvents.GetInstance().AnnotationAdded.Invoke(startPointForDrawing, endPointForDrawing);

                Vector3 distance = endPointForDrawing - startPointForDrawing;
                Vector3 scale = new Vector3(annotationWidth, annotationWidth, distance.magnitude );
                Vector3 midPosition = startPointForDrawing + (distance / 2.0f);
                GameObject currentAnnotation = annotations[annotations.Count - 1];
                currentAnnotation.transform.LookAt(endPointForDrawing);
                currentAnnotation.transform.position = midPosition;
                currentAnnotation.transform.localScale = scale;

                if (annotationLineHighlightPrefab)
                {
                    Vector3 highlightScale = new Vector3(annotationWidth * annotationHighlightWidthMultiplier, annotationWidth * annotationHighlightWidthMultiplier, distance.magnitude);
                    GameObject highlightObject = Instantiate(annotationLineHighlightPrefab);
                    highlightObject.transform.position = startPointForDrawing;
                    highlightObject.transform.LookAt(endPointForDrawing);
                    highlightObject.transform.position = midPosition * 1.005f;
                    highlightObject.transform.localScale = highlightScale;
                    
                    highlightObject.GetComponent<Renderer>().material.color =
                        SimulationManager.GetInstance().LocalPlayerColor;
                    
                    highlightObject.transform.parent = currentAnnotation.transform;
                }
                startPointForDrawing = Vector3.zero;
                endPointForDrawing = Vector3.zero;
            }
        }
        else
        {
            multipointLineDraw(nextPoint);
        }
    }

    public void AddAnnotation(Vector3 startPos, Vector3 endPos, Color playerColor)
    {
        Vector3 distance = endPos - startPos;
        Vector3 scale = new Vector3(annotationWidth, annotationWidth, distance.magnitude );
        Vector3 midPosition = startPos + (distance / 2.0f);
        GameObject currentAnnotation = Instantiate(annotationLinePrefab, startPos, Quaternion.identity, this.transform);
        currentAnnotation.transform.LookAt(endPointForDrawing);
        currentAnnotation.transform.position = midPosition;
        currentAnnotation.transform.localScale = scale;
        annotations.Add(currentAnnotation);

        if (annotationLineHighlightPrefab)
        {
            Vector3 highlightScale = new Vector3(annotationWidth * annotationHighlightWidthMultiplier, annotationWidth * annotationHighlightWidthMultiplier, distance.magnitude);
            GameObject highlightObject = Instantiate(annotationLineHighlightPrefab);
            highlightObject.transform.position = startPointForDrawing;
            highlightObject.transform.LookAt(endPointForDrawing);
            highlightObject.transform.position = midPosition * 1.005f;
            highlightObject.transform.localScale = highlightScale;
                    
            highlightObject.GetComponent<Renderer>().material.color =
                playerColor;
                    
            highlightObject.transform.parent = currentAnnotation.transform;
        }
    }
    
    private void multipointLineDraw(Vector3 nextPoint)
    {
        // line renderer
        if (startPointForDrawing == Vector3.zero)
        {
            startPointForDrawing = nextPoint;
            if (annotationLineRenderer.positionCount == 2 && annotationLineRenderer.GetPosition(0) == Vector3.zero)
            {
                annotationLineRenderer.SetPosition(0, startPointForDrawing);
                annotationLineRenderer.SetPosition(1, startPointForDrawing);
                annotationLinePoints.Add(startPointForDrawing);
                annotationLinePoints.Add(startPointForDrawing);
            }
            else
            {
                annotationLinePoints.Add(nextPoint);
            }
            annotationLineRenderer.positionCount = annotationLinePoints.Count;
            annotationLineRenderer.SetPositions(annotationLinePoints.ToArray());
        }
        else
        {
            annotationLinePoints.Add(nextPoint);
            annotationLineRenderer.positionCount = annotationLinePoints.Count;
            annotationLineRenderer.SetPositions(annotationLinePoints.ToArray());
        }
    }
    public void EndDrawingMode()
    {
        if (annotationLinePoints.Count > 2)
        {
            annotationLinePoints.RemoveAt(annotationLinePoints.Count - 1);
            annotationLineRenderer.positionCount = annotationLinePoints.Count;
            annotationLineRenderer.SetPositions(annotationLinePoints.ToArray());
        }
        startPointForDrawing = Vector3.zero;
        endPointForDrawing = Vector3.zero;
    }
}
