﻿using System;
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
    int layerMaskStars;
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
    OVRInputModule m_InputModule;
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

    private StarComponent currentStar;

    public AnnotationTool annotationTool;


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

        m_InputModule = FindObjectOfType<OVRInputModule>();

        layerMaskEarth = LayerMask.GetMask("Earth");
        layerMaskStars = LayerMask.GetMask("Stars");
        manager = SimulationManager.GetInstance();
        interactionController = FindObjectOfType<InteractionController>();

        laserLongDistance = (manager.SceneRadius + 2);
    }
    void Update()
    {
        if (manager == null) manager = SimulationManager.GetInstance();
        if (interactionController == null) interactionController = FindObjectOfType<InteractionController>();

        showIndicator(gameObject, shouldShowIndicator);
        toggleMenu();
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
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
            // if we're drawing, no need to raycast
            if (mainUIController.IsDrawing && annotationTool != null)
            {
                if (interactionTrigger())
                {
                    annotationTool.Annotate(laserEndPos);
                }

            }
            else
            {
                // Raycast, then if we hit the Earth or a star we can show the interaction
                ray = new Ray(laserStartPos, forwardDirection);

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
                                interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
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
                else if(allowStarInteractions && Physics.Raycast(ray, out hit, laserLongDistance, layerMaskStars))
                {
                    // Handle stars separately
                    StarComponent nextStar = hit.transform.GetComponent<StarComponent>();
                    if (nextStar != null)
                    {
                        if (nextStar != currentStar)
                        {
                            Debug.Log("New star! " + nextStar.starData.ProperName);
                            // remove highlighting from previous star
                            if (currentStar != null) currentStar.CursorHighlightStar(false);
                        }

                        currentStar = nextStar;
                        currentStar.CursorHighlightStar(true);

                        if (interactionTrigger())
                        {
                            currentStar.HandleSelectStar(true);
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
                }
            }
        }
    }
    void positionCanvasTransformRelativeToOrigin(GameObject canvasObject, float distance)
    {
        Transform origin = this.transform;
        if (m_InputModule != null && m_InputModule.rayTransform != null) origin = m_InputModule.rayTransform;
        canvasObject.transform.position = origin.position + (origin.forward * 6);
        Vector3 newRotation = Camera.main.transform.rotation.eulerAngles;
        newRotation.x = 0;
        newRotation.z = 0;
        canvasObject.transform.rotation = Quaternion.Euler(newRotation);
    }
    void toggleMenu()
    {
        if (leftMenuTrigger())
        {
            if (mainUI == null) mainUI = GameObject.Find("MainUI");
            if (mainUI)
            {
                positionCanvasTransformRelativeToOrigin(mainUI, 6f);
            }
        }
    }

    bool interactionTrigger()
    {
        return (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) ||
                        OVRInput.GetDown(OVRInput.Button.One) ||
                        OVRInput.GetDown(OVRInput.Button.Three));
    }
    bool grabTrigger()
    {
        return (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) ||
                        OVRInput.Get(OVRInput.Button.SecondaryHandTrigger));
    }
    bool leftMenuTrigger()
    {
        return (OVRInput.Get(OVRInput.Button.Two));
    }
    bool rightMenuTrigger()
    {
        return (OVRInput.Get(OVRInput.Button.Four));
    }

    void setDrawStartPoint()
    {

    }

    void updateLaser(bool activeTarget)
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
