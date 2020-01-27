using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject indicator;
    Camera camera;
    RaycastHit[] hits = new RaycastHit[2];
    RaycastHit hit;
    Ray ray;
    int layerMaskEarth;
    int layerMaskSphere;
    SimulationManager manager;
    InteractionController interactionController;
    public AnnotationTool annotationTool;
    MainUIController mainUIController;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskSphere = LayerMask.GetMask("InsideCelestialSphere");
        manager = SimulationManager.GetInstance();
        interactionController = FindObjectOfType<InteractionController>();
        mainUIController = FindObjectOfType<MainUIController>();
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
    }

    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();
        if (camera)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            // We have two colliders on Earth, so we collect data on all ray hits
            Physics.RaycastNonAlloc(ray, hits, 100.0F, layerMaskEarth);
    
            // Do something with the object that was hit by the raycast.
            if (Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    hit = hits[i];
                    Collider c = hit.collider;
                    if (c is SphereCollider)
                    {
                        // The Sphere collider is used for the Latitude / Longitude calculations
                        if (interactionController)
                        {
                            interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                        }
                        else
                        {
                            Debug.Log("💀 cant find interaction manager");
                        }
                    }
                    else if (c is MeshCollider)
                    {
                        Renderer rend = hit.transform.GetComponent<Renderer>();
                        if (rend == null)
                        {
                            Debug.Log("no renderer");
                            return;
                        } 
                        else
                        {
                            // hit.textureCoord only possible on the Mesh collider
                            manager.HorizonGroundColor = Utils.GetColorFromTexture(rend, hit.textureCoord);
                        }
                    }
                }
            }
            
        }
        if (mainUIController.IsDrawing && annotationTool)
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                float r = SimulationManager.GetInstance().SceneRadius + 2f;
                Vector2 mousePos = Input.mousePosition;
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, r));

                annotationTool.Annotate(Vector3.ClampMagnitude(pos, r));
            }
        }
    }
}
