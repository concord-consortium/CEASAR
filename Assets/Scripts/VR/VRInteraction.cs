using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

//public enum ControllerHand { Left, Right };
//[RequireComponent(typeof(LineRenderer))]
public class VRInteraction : MonoBehaviour
{
    RaycastHit hit;
    Ray ray;
    int layerMask;
    public GameObject vrPointerPrefab;

    private GameObject _vrPointer;
    public GameObject vrPointerInstance {
        get { return _vrPointer; }
    }
    LineRenderer laserLineRenderer;
    float laserMaxLength = 1500f;
    SimulationManager manager;
    InteractionController interactionController;
    bool canInteract = true;
    OVRInputModule m_InputModule;
    bool shouldShowIndicator;
    Vector3 laserStartPos = Vector3.zero;
    Vector3 forwardDirection = Vector3.forward;
    Vector3 laserEndPos = Vector3.zero;
    float activeLaserWidth = 0.04f;
    float inactiveLaserWidth = 0.02f;
    GameObject mainUI;
    GameObject networkUI;

    private StarComponent currentStar;

    private void Start()
    {

        shouldShowIndicator = SceneManager.GetActiveScene().name != "LoadSim"; // GameObject.Find("Earth") != null;
        _vrPointer = Instantiate(vrPointerPrefab);
        laserLineRenderer = _vrPointer.GetComponent<LineRenderer>();

        updateLaser(false);

        m_InputModule = FindObjectOfType<OVRInputModule>();

        layerMask = LayerMask.GetMask("Earth", "Stars");
        manager = SimulationManager.GetInstance();
        interactionController = FindObjectOfType<InteractionController>();
    }
    void Update()
    {
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
            laserEndPos = laserStartPos + (laserMaxLength * forwardDirection);
            updateLaser(false);
            // now detect if the user is holding down the Interact button also

            // raycast, then if we hit the Earth we can show the interaction
            ray = new Ray(laserStartPos, forwardDirection);
            if (Physics.Raycast(ray, out hit, laserMaxLength, layerMask))
            {
                laserEndPos = hit.point;
                updateLaser(true);

                if (hit.transform.name == "Earth")
                {
                    if (interactionTrigger())
                    {
                        if (canInteract)
                        {
                            // filter by what we hit

                            manager = SimulationManager.GetInstance();
                            interactionController = FindObjectOfType<InteractionController>();
                            interactionController.ShowEarthMarkerInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                            canInteract = false;
                        }
                    }
                    else
                    {
                        canInteract = true;
                    }
                }
                else if (hit.transform.tag == "Star")
                {
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
                            if (canInteract)
                            {
                                currentStar.HandleSelectStar(true);
                                canInteract = false;
                            }
                        }
                        else
                        {
                            canInteract = true;
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
                /*Transform origin = this.transform;
                if (m_InputModule != null && m_InputModule.rayTransform != null) origin = m_InputModule.rayTransform;
                mainUI.transform.position = origin.position + (origin.forward * 6);
                Vector3 newRotation = Camera.main.transform.rotation.eulerAngles;
                newRotation.x = 0;
                newRotation.z = 0;
                mainUI.transform.rotation = Quaternion.Euler(newRotation);*/
            }
        }
        if (rightMenuTrigger())
        {
            if (networkUI == null) networkUI = GameObject.Find("NetworkUI");
            if (networkUI)
            {
                positionCanvasTransformRelativeToOrigin(networkUI, 6f);
               /* Transform origin = this.transform;
                if (m_InputModule != null && m_InputModule.rayTransform != null) origin = m_InputModule.rayTransform;
                networkUI.transform.position = origin.position + (origin.forward * 6);
                Vector3 newRotation = Camera.main.transform.rotation.eulerAngles;
                newRotation.x = 0;
                newRotation.z = 0;
                networkUI.transform.rotation = Quaternion.Euler(newRotation);*/
            }
        }
    }

    bool interactionTrigger()
    {
        return (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) ||
                        OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) ||
                        OVRInput.Get(OVRInput.Button.One) ||
                        OVRInput.Get(OVRInput.Button.Three));
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
