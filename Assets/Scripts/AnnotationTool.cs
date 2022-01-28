using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationTool : MonoBehaviour
{
    private Vector3 startPointForDrawing = Vector3.zero;
    private Vector3 endPointForDrawing = Vector3.zero;

    public GameObject annotationLinePrefab;

    public float annotationWidth = 1;
    public float annotationHighlightWidthMultiplier = 1.5f;

    private GameObject currentAnnotation;
    private List<GameObject> myAnnotations;

    public Material annotationMaterial;

    public float annotationWidthMultiplier = 1f;
    public float scaleFactor = 1f;

    public bool IsMyAnnotation(GameObject annotationLine)
    {
        return myAnnotations.Contains(annotationLine);
    }

    public void Init()
    {
        myAnnotations = new List<GameObject>();
        SimulationEvents.Instance.AnnotationReceived.AddListener(AddAnnotation);
        SimulationEvents.Instance.AnnotationDeleted.AddListener(DeleteAnnotation);
        SimulationEvents.Instance.AnnotationClear.AddListener(ClearAnnotations);
    }

    private void OnDisable()
    {
        SimulationEvents.Instance.AnnotationReceived.RemoveListener(AddAnnotation);
        SimulationEvents.Instance.AnnotationDeleted.RemoveListener(DeleteAnnotation);
        SimulationEvents.Instance.AnnotationClear.RemoveListener(ClearAnnotations);
    }

    public void Annotate(Vector3 nextPoint)
    {
        if (annotationLinePrefab)
        {
            if (startPointForDrawing == Vector3.zero)
            {
                // start
                startPointForDrawing = nextPoint;
                currentAnnotation = Instantiate(annotationLinePrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
                currentAnnotation.GetComponent<AnnotationLine>().StartDrawing(startPointForDrawing * (1 / scaleFactor));

            }
            else if (endPointForDrawing == Vector3.zero)
            {
                // stretch most recent annotation to the end point
                endPointForDrawing = nextPoint;

                AddAnnotationLineRenderer(currentAnnotation, startPointForDrawing * (1 / scaleFactor), endPointForDrawing * (1 / scaleFactor), SimulationManager.Instance.LocalPlayerColor);

                currentAnnotation.GetComponent<AnnotationLine>().FinishDrawing();

                myAnnotations.Add(currentAnnotation);
                currentAnnotation.name = getMyAnnotationName(myAnnotations.Count);

                // Broadcast adding an annotation
                // send this as an annotation with points on a sphere at 100% zoom
                SimulationEvents.Instance.AnnotationAdded.Invoke(
                    startPointForDrawing * (1 / scaleFactor),
                    endPointForDrawing * (1 / scaleFactor),
                    currentAnnotation.transform.localEulerAngles,
                    currentAnnotation.name);

                startPointForDrawing = Vector3.zero;
                endPointForDrawing = Vector3.zero;
                currentAnnotation = null;

            }
        }
    }

    private void AddAnnotationLineRenderer(GameObject _currentAnnotation, Vector3 startPos, Vector3 endPos, Color playerColor)
    {
        _currentAnnotation.GetComponent<AnnotationLine>().StartPos = startPos;
        _currentAnnotation.GetComponent<AnnotationLine>().EndPos = endPos;
        _currentAnnotation.GetComponent<AnnotationLine>().RemoveStartPoint();

        int pointCount = 30;

        _currentAnnotation.layer = LayerMask.NameToLayer("Annotations");

        LineRenderer lineRendererArc = _currentAnnotation.AddComponent<LineRenderer>();
        MeshCollider meshColliderArc = _currentAnnotation.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lineRendererArc.useWorldSpace = false;
        lineRendererArc.startWidth = annotationWidth * annotationWidthMultiplier;
        lineRendererArc.endWidth = annotationWidth * annotationWidthMultiplier;
        lineRendererArc.material = annotationMaterial;

        lineRendererArc.positionCount = pointCount;
        lineRendererArc.material.color = playerColor;
        Vector3[] points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = Vector3.Slerp(startPos, endPos, (float)i / (pointCount - 1));
        }
        lineRendererArc.SetPositions(points);
        lineRendererArc.BakeMesh(mesh, false);
        meshColliderArc.sharedMesh = mesh;
        meshColliderArc.convex = true;
        meshColliderArc.isTrigger = true;
    }


    public void UndoAnnotation()
    {
        if (myAnnotations.Count > 0)
        {
            if (myAnnotations.Count == 1)
            {
                GameObject lineObject = myAnnotations[0];
                lineObject.GetComponent<AnnotationLine>().HandleDeleteAnnotation();
            }
            else
            {
                // we number the annotations as 1, 2, 3, 4 so no need to correct for 0-based array
                string annotationName = getMyAnnotationName(myAnnotations.Count);
                GameObject lineObject = myAnnotations.Find(a => a.name == annotationName);
                lineObject.GetComponent<AnnotationLine>().HandleDeleteAnnotation();
            }
        }
    }

    string getMyAnnotationName(int annotationNumber)
    {
        return SimulationManager.Instance.LocalUsername + "_annotation" + annotationNumber;
    }
    public void AddAnnotation(NetworkTransform lastAnnotation, NetworkPlayer p)
    {
        // This is a hack to use the NetworkTransform class to communicate the start/end positions
        // of the annotation. The NetworkTransform class is designed to store center, scale, and rotation,
        // but for now we will store the start and end positions of the annotation in the first and third vector3 slots.
        // Ideally we will clean this up and expand the network communication structures to handle the
        // annotations properly.
        Vector3 startPos = Utils.NetworkV3ToVector3(lastAnnotation.position);
        Vector3 rotation = Utils.NetworkV3ToVector3(lastAnnotation.rotation);
        Vector3 endPos = Utils.NetworkV3ToVector3(lastAnnotation.localScale);
        string annotationName = lastAnnotation.name;
        Color c = UserRecord.GetColorForUsername(p.username);
        // We receive this annotation at 100% zoom
        this.addAnnotation(startPos, endPos, rotation, annotationName, c);
    }

    private void addAnnotation(Vector3 startPos, Vector3 endPos, Vector3 rotation, string annotationName, Color playerColor)
    {
        GameObject newAnnotation = Instantiate(annotationLinePrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
        newAnnotation.transform.localEulerAngles = rotation;
        newAnnotation.name = annotationName;
        AddAnnotationLineRenderer(newAnnotation, startPos * (1 / scaleFactor), endPos * (1 / scaleFactor), playerColor);
    }

    void DeleteAnnotation(string annotationName)
    {
        GameObject deletedAnnotation = myAnnotations.Find(a => a.name == annotationName);
        if (deletedAnnotation != null)
        {
            myAnnotations.Remove(deletedAnnotation);
        }
    }
    public void SyncMyAnnotations()
    {
        for (int i = 0; i < myAnnotations.Count; i++)
        {
            float delay = SimulationManager.Instance.MovementSendInterval * i;
            // Need to delay sending each annotation so the network doesn't drop an update
            StartCoroutine(sendAnnotationDelayed(myAnnotations[i], delay));
        }
    }

    IEnumerator sendAnnotationDelayed(GameObject annotation, float delay)
    {
        yield return new WaitForSeconds(delay);
        SimulationEvents.Instance.AnnotationAdded.Invoke(annotation.GetComponent<AnnotationLine>().StartPos,
            annotation.GetComponent<AnnotationLine>().EndPos, annotation.transform.localEulerAngles, annotation.transform.name);
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

    public void EndDrawingMode()
    {
        if (startPointForDrawing != Vector3.zero && currentAnnotation != null && endPointForDrawing == Vector3.zero)
        {
            // we were in the middle of drawing when we stopped. Remove in-progress drawing
            Destroy(currentAnnotation);
        }
        startPointForDrawing = Vector3.zero;
        endPointForDrawing = Vector3.zero;
    }
}
