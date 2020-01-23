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
        SimulationEvents.GetInstance().AnnotationClear.AddListener(ClearAnnotations);
    }

    private void OnDisable()
    {
        SimulationEvents.GetInstance().AnnotationReceived.RemoveListener(AddAnnotation);
        SimulationEvents.GetInstance().AnnotationClear.RemoveListener(ClearAnnotations);
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
                
                
                Vector3 distance = endPointForDrawing - startPointForDrawing;
                Vector3 scale = new Vector3(annotationWidth, annotationWidth, distance.magnitude );
                Vector3 midPosition = startPointForDrawing + (distance / 2.0f);
                GameObject currentAnnotation = annotations[annotations.Count - 1];
                currentAnnotation.transform.LookAt(endPointForDrawing);
                currentAnnotation.transform.position = midPosition;
                currentAnnotation.transform.localScale = scale;
                currentAnnotation.name = SimulationManager.GetInstance().LocalUsername + "_annotation";
                
                // Broadcast adding an annotation
                SimulationEvents.GetInstance().AnnotationAdded.Invoke(currentAnnotation.transform.localPosition, currentAnnotation.transform.localRotation, currentAnnotation.transform.localScale);
                
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

    public void AddAnnotation(Vector3 pos, Quaternion rot, Vector3 scale, Player player)
    {
        Debug.Log("Received annotation " + pos + " " + rot + " " + scale);
        GameObject currentAnnotation = Instantiate(annotationLinePrefab, this.transform);
        currentAnnotation.transform.localPosition = pos;
        currentAnnotation.transform.localRotation = rot;
        currentAnnotation.transform.localScale = scale;
        currentAnnotation.name = player.username + "_annotation";
        annotations.Add(currentAnnotation);

        if (annotationLineHighlightPrefab)
        {
            GameObject highlightObject = Instantiate(annotationLineHighlightPrefab, currentAnnotation.transform);
            Transform ht = highlightObject.transform;
            ht.position *= 1.005f;
            ht.localScale = new Vector3(ht.localScale.x * annotationHighlightWidthMultiplier, ht.localScale.y * annotationHighlightWidthMultiplier, ht.localScale.z);
            highlightObject.GetComponent<Renderer>().material.color =
                SimulationManager.GetInstance().GetColorForUsername(player.username);
                    
        }
    }

    public void SyncMyAnnotations()
    {
        foreach (GameObject annotation in GameObject.FindGameObjectsWithTag("Annotation"))
        {
            if (annotation.name.StartsWith(SimulationManager.GetInstance().LocalUsername))
            {
                SimulationEvents.GetInstance().AnnotationAdded.Invoke(annotation.transform.localPosition, annotation.transform.localRotation, annotation.transform.localScale);
            }
        }
    }

    public void ClearAnnotations(string playerName)
    {
        foreach (GameObject annotation in GameObject.FindGameObjectsWithTag("Annotation"))
        {
            if (annotation.name.StartsWith(playerName))
            {
                Destroy(annotation);
            }
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
