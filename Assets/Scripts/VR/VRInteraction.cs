using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class VRInteraction : MonoBehaviour
{
    RaycastHit[] hits = new RaycastHit[2];
    RaycastHit hit;
    Ray ray;
    int layerMaskEarth;
    int layerMaskStarsAnnotations;
    private int layerMaskUI;
    public GameObject vrPointerPrefab;

    private GameObject _vrPointer;
    public GameObject vrPointerInstance {
        get { return _vrPointer; }
    }
    LineRenderer laserLineRenderer;
    float laserShortDistance = 40f;
    float laserLongDistance = 1500f;
    SimulationManager manager;
    InteractionController interactionController;
    bool canInteract = true;
#if !UNITY_WEBGL
    OVRInputModule m_InputModule;
#endif
    bool shouldShowIndicator;
    bool allowStarInteractions;
    Vector3 laserStartPos = Vector3.zero;
    Vector3 forwardDirection = Vector3.forward;
    Vector3 laserEndPos = Vector3.zero;
    float activeLaserWidth = 0.02f;
    float inactiveLaserWidth = 0.01f;
    GameObject mainUI;
    MainUIController mainUIController;
    GameObject networkUI;

    GameObject earthModel;

    private StarComponent currentStar;
    private AnnotationLine currentLine;

    public AnnotationTool annotationTool;

    private float menuDistance = 8f;

    #region Camera Move for Earth view
    private float distance = 20.0f;
    private float xSpeed = 0.2f;
    private float ySpeed = 0.4f;

    private float yMinLimit = -60f;
    private float yMaxLimit = 120f;

    float x = 0.0f;
    float y = 0.0f;
    #endregion


    LineRenderer drawingLine;
    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        shouldShowIndicator = sceneName != "LoadSim";
        allowStarInteractions = sceneName == "Horizon" || sceneName == "Stars";
        if (!annotationTool) annotationTool = FindObjectOfType<AnnotationTool>();
        _vrPointer = Instantiate(vrPointerPrefab);
        laserLineRenderer = _vrPointer.GetComponent<LineRenderer>();

        updateLaser(false);
#if !UNITY_WEBGL
        m_InputModule = FindObjectOfType<OVRInputModule>();
#endif
        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskStarsAnnotations = LayerMask.GetMask("Stars", "Annotations");
        layerMaskUI = LayerMask.GetMask("UI");
        manager = SimulationManager.GetInstance();
        interactionController = FindObjectOfType<InteractionController>();

        laserLongDistance = (manager.SceneRadius + 2);

        // test moving 
        positionCanvasTransformRelativeToOrigin(2);
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();

        showIndicator(gameObject, shouldShowIndicator);
        toggleMenu();
        moveAroundEarth();
    }
    private void OnDisable()
    {
        if (SimulationManager.GetInstance().CelestialSphereObject && !SimulationManager.GetInstance().CelestialSphereObject.activeSelf)
        {
            SimulationManager.GetInstance().CelestialSphereObject.SetActive(true);
        }
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
#if !UNITY_WEBGL
        if (showIndicator)
        {

            if (!m_InputModule) m_InputModule = FindObjectOfType<OVRInputModule>();

            Transform rayOrigin = this.transform;

            if (m_InputModule != null && m_InputModule.rayTransform != null) rayOrigin = m_InputModule.rayTransform;

            laserStartPos = rayOrigin.position;
            forwardDirection = rayOrigin.forward;
            laserEndPos = laserStartPos + (laserLongDistance * forwardDirection);
            updateLaser(false);
            // Next, detect if the user is holding down the Interact button also

            if (mainUIController == null) mainUIController = FindObjectOfType<MainUIController>();

            // Raycast, then if we hit the Earth or a star we can show the interaction
            ray = new Ray(laserStartPos, forwardDirection);

            // Look for close-by objects (Earth in the EarthInteraction scene)
            if (Physics.RaycastNonAlloc(ray, hits, laserShortDistance, layerMaskEarth) > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    hit = hits[i];
                    laserEndPos = hit.point;
                    updateLaser(true);

                    if (interactionTrigger())
                    {
                        Collider c = hit.collider;
                        if (c is SphereCollider)
                        {
                            if (mainUIController.IsPinningLocation)
                            {
                                interactionController.SetEarthLocationPin(hit.point);
                            }
                            else
                            {
                                interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                            }
                        }
                        else if (c is MeshCollider)
                        {
                            Renderer rend = hit.transform.GetComponent<Renderer>();
                            // hit.textureCoord only possible on the Mesh collider
                            manager.HorizonGroundColor = Utils.GetColorFromTexture(rend, hit.textureCoord);
                        }
                    }
                }
            }
            // Look for distant objects (stars in other views)
            else if (allowStarInteractions && Physics.Raycast(ray, out hit, laserLongDistance, layerMaskStarsAnnotations))
            {
                // Handle stars separately
                StarComponent nextStar = hit.transform.GetComponent<StarComponent>();
                if (nextStar != null)
                {
                    
                    if (nextStar != currentStar)
                    {
                        // remove highlighting from previous star
                        if (currentStar != null) currentStar.CursorHighlightStar(false);
                        // remove highlighting from previously-hovered annotation
                        if (currentLine != null)
                        {
                            currentLine.Highlight(false);
                            currentLine = null;
                        }
                        // vibrate only on new star highlight
                        hapticFeedback();
                    }

                    currentStar = nextStar;
                    currentStar.CursorHighlightStar(true);

                    if (interactionTrigger())
                    {
                        hapticFeedback();
                        if (mainUIController.IsDrawing && annotationTool != null)
                        {
                            // allow annotation where the star is
                            annotationTool.Annotate(laserEndPos);
                        }
                        else
                        {
                            // select the star
                            currentStar.HandleSelectStar(true);
                        }

                    }
                }
                else
                {
                    AnnotationLine nextLine = hit.transform.GetComponent<AnnotationLine>();
                    // we hit an annotation
                    if (nextLine != null)
                    {
                        if (nextLine != currentLine)
                        {
                            // remove highlighting from previous line
                            if (currentLine != null) currentLine.Highlight(false);
                            hapticFeedback();
                        }
                        currentLine = nextLine;
                        if (!mainUIController.IsDrawing)
                        {
                            // When we're not drawing we can highlight and delete annotations
                            currentLine.Highlight(true);

                            if (interactionFingerTrigger())
                            {
                                // delete the line
                                currentLine.HandleDeleteAnnotation();
                                currentLine = null;
                                hapticFeedback();
                            } 
                           
                        }
                        
                    }
                }
            }
            else
            {
                if (currentStar != null)
                {
                    currentStar.CursorHighlightStar(false);
                }
                updateLaser(false);

                if (interactionTrigger())
                {
                    if (mainUIController.IsDrawing && annotationTool != null && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Seems like a crazy way to determine if the UI is in the way, but after multiple attempts at a more
                        // elegant solution, settled on this since it works.

                        if (FindObjectOfType<LaserPointer>().GetComponent<LineRenderer>().enabled)

                        {
                            // Blocked by the UI layer
                            return;
                        }
                        else
                        {
                            // allow annotation where the star is
                            annotationTool.Annotate(laserEndPos);
                        }

                    }
                }
            }

        }
#endif
    }
    void positionCanvasTransformRelativeToOrigin(float verticalOffset = 0)
    {
#if !UNITY_WEBGL
        Transform origin = this.transform;
        if (mainUI == null) mainUI = GameObject.FindGameObjectWithTag("MainUI");
        
        if (m_InputModule != null && m_InputModule.rayTransform != null) origin = m_InputModule.rayTransform;

        // one last null check - this will be the case on LoadSim
        if (mainUI != null)
        {
            Vector3 newRotation = origin.transform.rotation.eulerAngles;
            newRotation.x = 0;
            newRotation.z = 0;
            newRotation.y += 30;
            mainUI.transform.rotation = Quaternion.Euler(newRotation);
            Vector3 pos = origin.position + (origin.forward * menuDistance);
            pos.y += verticalOffset;
            mainUI.transform.position = pos;
        }
#endif
    }
    void toggleMenu()
    {
        if (menuButton())
        {
            positionCanvasTransformRelativeToOrigin();
        }
    }

    void moveAroundEarth()
    {
#if !UNITY_WEBGL
        if (SceneManager.GetActiveScene().name == "EarthInteraction")
        {
            if (!earthModel) earthModel = GameObject.Find("EarthContainer");
           
            if (grabTrigger())
            {
                if (mainUI == null) mainUI = GameObject.Find("MainUI");
                if (mainUI && mainUI.transform.parent == null)
                {
                    mainUI.transform.parent = this.transform;
                }
                
                // rotate VR camera around Earth
                float distance = Vector3.Magnitude(transform.position - earthModel.transform.position);

                x += OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x * xSpeed * distance;
                y -= OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * ySpeed;
                x += OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x * xSpeed * distance;
                y -= OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y * ySpeed;

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + earthModel.transform.position;
                if (rotation != transform.rotation || position != transform.position)
                {
                    SimulationManager.GetInstance().CelestialSphereObject.SetActive(false);
                    transform.rotation = rotation;
                    transform.position = position;
                } else
                {
                    if (!SimulationManager.GetInstance().CelestialSphereObject.activeSelf)
                    {
                        SimulationManager.GetInstance().CelestialSphereObject.SetActive(true);
                    }
                }
                
            } else
            {
                if (!SimulationManager.GetInstance().CelestialSphereObject.activeSelf)
                {
                    SimulationManager.GetInstance().CelestialSphereObject.SetActive(true);
                }
               
            }
        }
#endif
    }
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
    bool interactionTrigger(bool oneShot = true)
    {
#if !UNITY_WEBGL
        if (oneShot)
        return (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.One) ||
                        OVRInput.GetDown(OVRInput.Button.Three));
        else
            return (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) ||
                        OVRInput.Get(OVRInput.Button.One) ||
                        OVRInput.Get(OVRInput.Button.Three));
#else 
        return false;
#endif
    }
    bool interactionFingerTrigger()
    {
#if !UNITY_WEBGL
        return (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
#else 
        return false;
#endif
    }
    bool grabTrigger()
    {
#if !UNITY_WEBGL
        return (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) ||
                        OVRInput.Get(OVRInput.Button.SecondaryHandTrigger));
#else 
        return false;
#endif
    }
    bool leftMenuTrigger()
    {
#if !UNITY_WEBGL
        return (OVRInput.Get(OVRInput.Button.Two));
#else 
        return false;
#endif
    }
    bool rightMenuTrigger()
    {
#if !UNITY_WEBGL
        return (OVRInput.Get(OVRInput.Button.Four));
#else 
        return false;
#endif
    }
    bool menuButton()
    {
        return leftMenuTrigger() || rightMenuTrigger();
    }

    void setDrawStartPoint()
    {

    }

    void hapticFeedback()
    {
#if !UNITY_WEBGL
        OVRInput.SetControllerVibration(0.1f, 0.2f, OVRInput.GetActiveController());
        StartCoroutine(stopHaptic());
#endif
    }
    IEnumerator stopHaptic()
    {
#if !UNITY_WEBGL
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.GetActiveController());
#else
        yield return new WaitForSeconds(0.1f);
#endif
    }

    void updateLaser(bool activeTarget)
    {
        if (SceneManager.GetActiveScene().name != SimulationConstants.SCENE_EARTH)
        {
            laserLineRenderer.SetPosition(0, laserStartPos);
            laserLineRenderer.SetPosition(1, laserEndPos);
            if (activeTarget)
            {
                laserLineRenderer.startWidth = activeLaserWidth;
                laserLineRenderer.endWidth = activeLaserWidth;
            }
            else
            {
                laserLineRenderer.startWidth = inactiveLaserWidth;
                laserLineRenderer.endWidth = inactiveLaserWidth;
            }
        }
    }
}
