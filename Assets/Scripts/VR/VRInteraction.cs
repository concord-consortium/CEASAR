using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//public enum ControllerHand { Left, Right };
//[RequireComponent(typeof(LineRenderer))]
public class VRInteraction : MonoBehaviour
{
    RaycastHit hit;
    Ray ray;
    int layerMask;
    public GameObject vrPointerPrefab;
    LineRenderer laserLineRenderer;
    //public float laserWidth = 0.1f;
    public float laserMaxLength = 15f;
    SimulationManager manager;
    NetworkController network;
    //public ControllerHand hand = ControllerHand.Left;
    Vector3[] initLaserPositions;
    bool canInteract = true;
    OVRInputModule m_InputModule;
    bool shouldShowIndicator;

    private void Start()
    {
        shouldShowIndicator = GameObject.Find("Earth") != null;
        laserLineRenderer = Instantiate(vrPointerPrefab).GetComponent<LineRenderer>();
        initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer.SetPositions(initLaserPositions);
        //laserLineRenderer.startWidth = laserWidth;
        //laserLineRenderer.endWidth = laserWidth;
        m_InputModule = FindObjectOfType<OVRInputModule>();

        layerMask = LayerMask.GetMask("Earth");
        manager = SimulationManager.GetInstance();
        network = FindObjectOfType<NetworkController>();
    }
    void Update()
    {
        // OVRInput.Update();

        showIndicator(gameObject, shouldShowIndicator);
        //if (hand == ControllerHand.Left) {
        //    showIndicator(gameObject, OVRInput.Get(OVRInput.Button.Three));
        //} else { 
        //    showIndicator(gameObject, OVRInput.Get(OVRInput.Button.One));
        //}
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
        // MeshRenderer renderer = controllerObject.GetComponentInChildren<MeshRenderer>();

        if (showIndicator)
        {   
            if(!m_InputModule) m_InputModule = FindObjectOfType<OVRInputModule>();
            Transform rayOrigin = this.transform;
            if (m_InputModule != null && m_InputModule.rayTransform != null) rayOrigin = m_InputModule.rayTransform;
            Vector3 pos = rayOrigin.position;
            Vector3 forwardDirection = rayOrigin.forward;
            Vector3 endPosition = pos + (laserMaxLength * forwardDirection);

            laserLineRenderer.SetPosition(0, pos);
            laserLineRenderer.SetPosition(1, endPosition);

            // now detect if the user is holding down the Interact button also
            // bool canInteract = Time.time - lastInteract > interactionInterval;

            // raycast, then if we hit the Earth we can show the interaction
            ray = new Ray(pos, forwardDirection);
            if (Physics.Raycast(ray, out hit, laserMaxLength, layerMask))
            {
                laserLineRenderer.SetPosition(1, hit.point);
                if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
                {
                    if (canInteract)
                    {
                        // filter by what we hit
                        if (hit.transform.name == "Earth")
                        {
                            manager = SimulationManager.GetInstance();
                            network = FindObjectOfType<NetworkController>();
                            network.ShowInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                            canInteract = false;
                        } else
                        {
                            // handle star interaction, etc
                        }
                    }
                }
                else
                {
                    canInteract = true;
                }
            }
        }
        else
        {
            laserLineRenderer.SetPositions(initLaserPositions);
        }
    }
    private void FixedUpdate()
    {
        // OVRInput.FixedUpdate();
    }
}
