using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject indicator;
    Camera camera;
    RaycastHit[] hits;
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

    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();
        if (camera)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            // We have two colliders on Earth, so we collect data on all ray hits
            hits = Physics.RaycastAll(ray, 100.0F, layerMaskEarth);
    
            // Do something with the object that was hit by the raycast.
            if (Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];
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
                        Texture2D tex = rend.material.GetTexture("_MainTex") as Texture2D;
                        Vector2 pixelUV = hit.textureCoord; // Only possible on the Mesh collider
                        pixelUV.x *= tex.width;
                        pixelUV.y *= tex.height;
                        // apply our offset on X axis
                        pixelUV.x += ( rend.material.GetTextureOffset("_MainTex").x * tex.width);
                        
                        Color pixelColor = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
                        Debug.Log(pixelColor);
                        manager.HorizonGroundColor = pixelColor;
                    }
                    
                   
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
