using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerHand {  Left, Right };
[RequireComponent(typeof(LineRenderer))]
public class VRInteraction : MonoBehaviour
{
    RaycastHit hit;
    Ray ray;
    int layerMask;
    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.1f;
    public float laserMaxLength = 15f;
    SimulationManager manager;
    NetworkController network;
    public ControllerHand hand = ControllerHand.Left;
    Vector3[] initLaserPositions;
    bool canInteract = true;
    private void Start()
    {
        initLaserPositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        laserLineRenderer = this.GetComponent<LineRenderer>();
        laserLineRenderer.SetPositions(initLaserPositions);
        laserLineRenderer.startWidth = laserWidth;
        laserLineRenderer.endWidth = laserWidth;
        layerMask = LayerMask.GetMask("Earth");
        manager = SimulationManager.GetInstance();
        network = FindObjectOfType<NetworkController>();
    }
    void Update()
    {
        OVRInput.Update();
        if (hand == ControllerHand.Left) {
            showIndicator(gameObject, OVRInput.Get(OVRInput.Button.Three));
        } else { 
            showIndicator(gameObject, OVRInput.Get(OVRInput.Button.One));
        }
    }

    void showIndicator(GameObject controllerObject, bool showIndicator)
    {
        MeshRenderer renderer = controllerObject.GetComponentInChildren<MeshRenderer>();
        if (renderer)
        {
            renderer.enabled = showIndicator;
        }
        if (showIndicator)
        {
            Vector3 pos = transform.position;
            Vector3 forwardDirection = transform.forward;
            Vector3 endPosition = pos + (laserMaxLength * forwardDirection);

            laserLineRenderer.SetPosition(0, pos);
            laserLineRenderer.SetPosition(1, endPosition);

            // now detect if the user is holding down the Interact button also
            // bool canInteract = Time.time - lastInteract > interactionInterval;
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (canInteract)
                {
                    // raycast, then if we hit the Earth we can show the interaction
                    ray = new Ray(pos, forwardDirection);
                    if (Physics.Raycast(ray, out hit, laserMaxLength, layerMask))
                    {
                        manager = SimulationManager.GetInstance();
                        network = FindObjectOfType<NetworkController>();
                        network.ShowInteraction(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), manager.LocalPlayerColor, true);
                        canInteract = false;
                    }
                }
            } else
            {
                canInteract = true;
            }
            
        } else
        {
            laserLineRenderer.SetPositions(initLaserPositions);
        }
    }

    private void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }
}
