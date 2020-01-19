using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject indicator;
    Camera camera;
    RaycastHit hit;
    Ray ray;
    int layerMaskEarth;
    int layerMaskSphere;
    SimulationManager manager;
    InteractionController interactionController;
    public AnnotationTool annotationTool;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskSphere = LayerMask.GetMask("InsideCelestialSphere");
        manager = SimulationManager.GetInstance();
        interactionController = FindObjectOfType<InteractionController>();
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        if (camera)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100, layerMaskEarth))
            {
                // Do something with the object that was hit by the raycast.
                if (Input.GetMouseButtonDown(0))
                {
                    manager = SimulationManager.GetInstance();
                    interactionController = FindObjectOfType<InteractionController>();
                    if (interactionController)
                    {
                        interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                    }
                    else
                    {
                        Debug.Log("💀 cant find interaction manager");
                    }

                }
            }
            if (annotationTool)
            {
                if (Physics.Raycast(ray, out hit, manager.SceneRadius + 2, layerMaskSphere))
                {
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                     
                        Debug.Log("Inside of sphere " + hit.point);
                        annotationTool.Annotate(hit.point);// camera.ScreenToWorldPoint(Input.mousePosition));
                    }
                }
            }
        }
    }


}
