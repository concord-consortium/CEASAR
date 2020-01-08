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
    LineRenderer laserLineRenderer;
    float laserMaxLength = 1500f;
    SimulationManager manager;
    NetworkController network;
    bool canInteract = true;
    OVRInputModule m_InputModule;
    bool shouldShowIndicator;
    Vector3 laserStartPos = Vector3.zero;
    Vector3 forwardDirection = Vector3.forward;
    Vector3 laserEndPos = Vector3.zero;
    float activeLaserWidth = 0.04f;
    float inactiveLaserWidth = 0.02f;

    private StarComponent currentStar;

    private void Start()
    {

        shouldShowIndicator = SceneManager.GetActiveScene().name != "LoadSim"; // GameObject.Find("Earth") != null;
        laserLineRenderer = Instantiate(vrPointerPrefab).GetComponent<LineRenderer>();

        updateLaser(false);

        m_InputModule = FindObjectOfType<OVRInputModule>();

        layerMask = LayerMask.GetMask("Earth", "Stars");
        manager = SimulationManager.GetInstance();
        network = FindObjectOfType<NetworkController>();
    }
    void Update()
    {
        showIndicator(gameObject, shouldShowIndicator);
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
        // MeshRenderer renderer = controllerObject.GetComponentInChildren<MeshRenderer>();

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
            // bool canInteract = Time.time - lastInteract > interactionInterval;

            // raycast, then if we hit the Earth we can show the interaction
            ray = new Ray(laserStartPos, forwardDirection);
            if (Physics.Raycast(ray, out hit, laserMaxLength, layerMask))
            {
                laserEndPos = hit.point;
                updateLaser(true);

                if (hit.transform.name == "Earth")
                {
                    if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
                    {
                        if (canInteract)
                        {
                            // filter by what we hit

                            manager = SimulationManager.GetInstance();
                            network = FindObjectOfType<NetworkController>();
                            network.ShowInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
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

                        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
                        {
                            if (canInteract)
                            {
                                currentStar.HandleSelectStar();
                                manager = SimulationManager.GetInstance();
                                network = FindObjectOfType<NetworkController>();
                                network.ShowInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
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
