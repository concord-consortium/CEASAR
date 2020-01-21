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
    public float annotationWidth = 1;
    private List<GameObject> annotations;

    private LineRenderer annotationLineRenderer;
    private List<Vector3> annotationLinePoints;
    private void Start()
    {
        annotationLineRenderer = this.GetComponent<LineRenderer>();
        annotations = new List<GameObject>();
        annotationLinePoints = new List<Vector3>();
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
                Vector3 offset = endPointForDrawing - startPointForDrawing;
                Vector3 scale = new Vector3(annotationWidth, offset.magnitude, annotationWidth);
                Vector3 midPosition = startPointForDrawing + (offset / 2.0f);
                GameObject currentAnnotation = annotations[annotations.Count - 1];
                currentAnnotation.transform.position = midPosition;
                currentAnnotation.transform.up = offset;
                currentAnnotation.transform.localScale = scale;
                startPointForDrawing = Vector3.zero;
                endPointForDrawing = Vector3.zero;
            }
        }
        else
        {
            // fallback to line renderer
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
    }
    public void CleanLastLinePosition()
    {
        annotationLinePoints.RemoveAt(annotationLinePoints.Count - 1);
        annotationLineRenderer.positionCount = annotationLinePoints.Count;
        annotationLineRenderer.SetPositions(annotationLinePoints.ToArray());
        startPointForDrawing = Vector3.zero;
    }
}
