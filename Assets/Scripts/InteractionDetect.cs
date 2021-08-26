using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionDetect : MonoBehaviour
{
    public GameObject indicator;
    Camera camera;
    RaycastHit[] hits = new RaycastHit[2];
    RaycastHit hit;
    Ray ray;
    private int layerMaskEarth;
    private int layerMaskStarsAnnotations;
    SimulationManager manager;
    InteractionController interactionController;
    public AnnotationTool annotationTool;
    MenuController mainUIController;
    private PushpinComponent lastPin;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskStarsAnnotations = LayerMask.GetMask("Stars", "Annotations");
        manager = SimulationManager.Instance;
        interactionController = FindObjectOfType<InteractionController>();
        mainUIController = FindObjectOfType<MenuController>();
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
    }

    void Update()
    {
        if (manager == null) manager = SimulationManager.Instance;
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();
        if (camera)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            // We have two colliders on Earth, so we collect data on all ray hits
            Physics.RaycastNonAlloc(ray, hits, 100.0F, layerMaskEarth);

            // Do something with the object that was hit by the raycast.
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() )
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
                            // Default is now set so that pinning = true all the time - leaving this in place for future
                            if (mainUIController.IsPinningLocation)
                            {
                                // Add or move the local player pin to the new point
                                // Rotation is calculated because we're using a sphere so it's always facing out from center
                                interactionController.SetEarthLocationPin(hit.point);
                            }
                            else
                            {
                                interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                            }

                        }
                        else
                        {
                            CCDebug.Log("💀 cant find interaction manager", LogLevel.Error, LogMessageCategory.Interaction);
                        }
                    }
                    else if (c is MeshCollider)
                    {
                        Renderer rend = hit.transform.GetComponent<Renderer>();
                        if (rend == null)
                        {
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
        if (mainUIController && mainUIController.IsDrawing && annotationTool)
        {
            ray = camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, manager.SceneRadius);// layerMaskStarsAnnotations);
            if (Input.GetMouseButtonDown(0))
            {
                if ((!EventSystem.current.IsPointerOverGameObject() || hit.point != Vector3.zero) && !IsPointerOverUIElement())
                {
                    float r = SimulationManager.Instance.SceneRadius + 2f;
                    Vector2 mousePos = Input.mousePosition;
                    Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, r));

                    annotationTool.Annotate(Vector3.ClampMagnitude(pos, r));
                }
            }
        }
    }

    // Returns 'true' if we touched or are hovering over Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    // Returns 'true' if we touched or are hovering over Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        for (int index = 0;  index < eventSystemRaycastResults.Count; index ++)
        {
            RaycastResult curRaycastResult = eventSystemRaycastResults [index];
            if (curRaycastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
    // Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }
}
