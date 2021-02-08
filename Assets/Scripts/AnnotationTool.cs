using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationTool : MonoBehaviour
{
    private Vector3 startPointForDrawing = Vector3.zero;
    private Vector3 endPointForDrawing = Vector3.zero;
    
    public GameObject annotationLinePrefab;
    public GameObject annotationLineHighlightPrefab;
    
    public float annotationWidth = 1;
    public float annotationHighlightWidthMultiplier = 1.5f;

    private GameObject currentAnnotation;
    private List<GameObject> myAnnotations;

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
                currentAnnotation = Instantiate(annotationLinePrefab, startPointForDrawing, Quaternion.identity, this.transform);
                
            }
            else if (endPointForDrawing == Vector3.zero)
            {
                // stretch most recent annotation to the end point
                endPointForDrawing = nextPoint;

                Vector3 distance = endPointForDrawing - startPointForDrawing;
                Vector3 scale = new Vector3(annotationWidth, annotationWidth, distance.magnitude );
                Vector3 midPosition = startPointForDrawing + (distance / 2.0f);
                
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
                        SimulationManager.Instance.LocalPlayerColor;
                    
                    highlightObject.transform.parent = currentAnnotation.transform;
                }
                currentAnnotation.GetComponent<AnnotationLine>().FinishDrawing();
                myAnnotations.Add(currentAnnotation);
                currentAnnotation.name = getMyAnnotationName(myAnnotations.Count);
                
                // Broadcast adding an annotation
                SimulationEvents.Instance.AnnotationAdded.Invoke(
                    currentAnnotation.transform.localPosition, 
                    currentAnnotation.transform.localRotation, 
                    currentAnnotation.transform.localScale, 
                    currentAnnotation.name);
                
                startPointForDrawing = Vector3.zero;
                endPointForDrawing = Vector3.zero;
                currentAnnotation = null;
            }
        }
    }
    public void UndoAnnotation()
    {
        if (myAnnotations.Count > 0)
        {
            string annotationName = getMyAnnotationName(myAnnotations.Count - 1);
            GameObject lineObject = myAnnotations.Find(a => a.name == annotationName);
            lineObject.GetComponent<AnnotationLine>().HandleDeleteAnnotation();
        }
    }

    string getMyAnnotationName(int annotationNumber)
    {
        return SimulationManager.Instance.LocalUsername + "_annotation" + annotationNumber;
    }
    public void AddAnnotation(NetworkTransform lastAnnotation, NetworkPlayer p)
    {
        Vector3 pos = Utils.NetworkV3ToVector3(lastAnnotation.position);
        Quaternion rot = Utils.NetworkV3ToQuaternion(lastAnnotation.rotation);
        Vector3 scale = Utils.NetworkV3ToVector3(lastAnnotation.localScale);
        string annotationName = lastAnnotation.name;
        Color c = UserRecord.GetColorForUsername(p.username);
        this.addAnnotation(pos, rot, scale, annotationName, c);
        
    }
    private void addAnnotation(Vector3 pos, Quaternion rot, Vector3 scale, string annotationName, Color playerColor)
    {
        GameObject currentAnnotation = Instantiate(annotationLinePrefab, this.transform);
        currentAnnotation.transform.localPosition = pos;
        currentAnnotation.transform.localRotation = rot;
        currentAnnotation.transform.localScale = scale;
        currentAnnotation.name = annotationName;

        if (annotationLineHighlightPrefab)
        {
            GameObject highlightObject = Instantiate(annotationLineHighlightPrefab, currentAnnotation.transform);
            Transform ht = highlightObject.transform;
            ht.position *= 1.005f;
            ht.localScale = new Vector3(ht.localScale.x * annotationHighlightWidthMultiplier, ht.localScale.y * annotationHighlightWidthMultiplier, ht.localScale.z);
            highlightObject.GetComponent<Renderer>().material.color = playerColor;
        }
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
        SimulationEvents.Instance.AnnotationAdded.Invoke(annotation.transform.localPosition,
            annotation.transform.localRotation, annotation.transform.localScale, annotation.transform.name);
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
