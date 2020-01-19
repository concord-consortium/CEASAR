using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AnnotationTool : MonoBehaviour
{
    private Vector3 startPointForDrawing = Vector3.zero;
    private Vector3 endPointForDrawing = Vector3.zero;
    private LineRenderer annotationLineRenderer;
    private void Start()
    {
        annotationLineRenderer = this.GetComponent<LineRenderer>();
    }
    public void Annotate(Vector3 nextPoint)
    {
        if (startPointForDrawing == Vector3.zero)
        {
            // first interaction sets start
            startPointForDrawing = nextPoint;
            Debug.Log("Start point set: " + startPointForDrawing.ToString());
            annotationLineRenderer.positionCount = 2;
            annotationLineRenderer.SetPosition(0, startPointForDrawing);
            annotationLineRenderer.SetPosition(1, startPointForDrawing);

        }
        else if (endPointForDrawing == Vector3.zero)
        {
            annotationLineRenderer.SetPosition(1, nextPoint);
            Debug.Log("End point set: " + endPointForDrawing.ToString() + " magnitude: " + Vector3.Magnitude(endPointForDrawing - startPointForDrawing));
        }
        else
        {
            // clear start/end
            startPointForDrawing = Vector3.zero;
            endPointForDrawing = Vector3.zero;
            annotationLineRenderer.SetPosition(0, Vector3.zero);
            annotationLineRenderer.SetPosition(1, Vector3.zero);

        }
    }
}
