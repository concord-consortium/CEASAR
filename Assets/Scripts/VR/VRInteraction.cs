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
    private bool shouldShowIndicator = true;
    bool allowStarInteractions;
    Vector3 laserStartPos = Vector3.zero;
    Vector3 forwardDirection = Vector3.forward;
    Vector3 laserEndPos = Vector3.zero;
    float activeLaserWidth = 0.02f;
    float inactiveLaserWidth = 0.01f;
    GameObject mainUI;
    private GameObject infoPanelUI;
    MenuController menuController;

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
        allowStarInteractions = sceneName == SimulationConstants.SCENE_HORIZON || sceneName == SimulationConstants.SCENE_STARS;
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
        manager = SimulationManager.Instance;
        interactionController = FindObjectOfType<InteractionController>();

        laserLongDistance = (manager.SceneRadius + 2);

        // test moving 
        if (mainUI == null) mainUI = GameObject.FindGameObjectWithTag("MainUI");
        if (infoPanelUI == null) infoPanelUI = GameObject.FindGameObjectWithTag("InfoPanelUI");
        if (mainUI)
        {
            positionCanvasTransformRelativeToOrigin(mainUI, 1.5f);
        }

        
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.Instance;
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();

        showIndicator(gameObject, shouldShowIndicator);
        toggleMenu();
        moveAroundEarth();
    }
    private void OnDisable()
    {
        if (SimulationManager.Instance.CelestialSphereObject && !SimulationManager.Instance.CelestialSphereObject.activeSelf)
        {
            SimulationManager.Instance.CelestialSphereObject.SetActive(true);
        }
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
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

            if (menuController == null) menuController = FindObjectOfType<MenuController>();

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
                            if (menuController.IsPinningLocation)
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
                        if (menuController.IsDrawing && annotationTool != null)
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
                        if (!menuController.IsDrawing)
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
                    if (menuController.IsDrawing && annotationTool != null && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // allow annotation
                        annotationTool.Annotate(laserEndPos);
                    }
                }
            }

        }
#endif
    }
    void positionCanvasTransformRelativeToOrigin(GameObject canvasObject, float verticalOffset = 0)
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        Transform origin = this.transform;
        
        if (m_InputModule != null && m_InputModule.rayTransform != null) origin = m_InputModule.rayTransform;
        
        Vector3 pos = origin.position + (origin.forward * menuDistance);
        pos.y += verticalOffset;
        canvasObject.transform.position = pos;
        
        Vector3 camPos = Camera.main.transform.position;
        Vector3 relativePos = pos - camPos;
        canvasObject.transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);
#endif
    }
    void toggleMenu()
    {
        if (menuButton() && mainUI)
        {
            positionCanvasTransformRelativeToOrigin(mainUI);
        }

        if (infoPanelButton() && infoPanelUI)
        {
            positionCanvasTransformRelativeToOrigin(infoPanelUI);
        }
    }

    void moveAroundEarth()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_EARTH)
        {
            if (!earthModel) earthModel = GameObject.Find("EarthContainer");
           
            if (grabTrigger())
            {
                if (mainUI == null) mainUI = GameObject.FindGameObjectWithTag("MainUI");
                if (infoPanelUI == null) infoPanelUI = GameObject.FindGameObjectWithTag("InfoPanelUI");
                if (mainUI && mainUI.transform.parent == null)
                {
                    mainUI.transform.parent = this.transform;
                }
                if (infoPanelUI && infoPanelUI.transform.parent == null)
                {
                    infoPanelUI.transform.parent = this.transform;
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
                    SimulationManager.Instance.CelestialSphereObject.SetActive(false);
                    transform.rotation = rotation;
                    transform.position = position;
                } else
                {
                    if (!SimulationManager.Instance.CelestialSphereObject.activeSelf)
                    {
                        SimulationManager.Instance.CelestialSphereObject.SetActive(true);
                    }
                }
                
            } else
            {
                if (!SimulationManager.Instance.CelestialSphereObject.activeSelf)
                {
                    SimulationManager.Instance.CelestialSphereObject.SetActive(true);
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
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
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
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        return (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger));
#else 
        return false;
#endif
    }
    bool grabTrigger()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        return (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) ||
                        OVRInput.Get(OVRInput.Button.SecondaryHandTrigger));
#else 
        return false;
#endif
    }
    bool leftMenuTrigger()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        return (OVRInput.Get(OVRInput.Button.Two));
#else 
        return false;
#endif
    }
    bool rightMenuTrigger()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        return (OVRInput.Get(OVRInput.Button.Four));
#else 
        return false;
#endif
    }
    bool menuButton()
    {
        return leftMenuTrigger();
    }

    bool infoPanelButton()
    {
        return rightMenuTrigger();
    }

    void setDrawStartPoint()
    {

    }

    void hapticFeedback()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        OVRInput.SetControllerVibration(0.1f, 0.2f, OVRInput.GetActiveController());
        StartCoroutine(stopHaptic());
#endif
    }
    IEnumerator stopHaptic()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
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
