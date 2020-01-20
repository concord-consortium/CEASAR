using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 lastPos;
    private Quaternion lastRot;
    private float lastSend = 0;
    NetworkController network;
    public bool useLocalRotation = false;
    private void Awake()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void FixedUpdate()
    {
        if (lastPos != transform.position || lastRot != transform.rotation)
        {
            // send update - no more frequently than once per second
            if (Time.time - SimulationManager.GetInstance().MovementSendInterval > lastSend)
            {
                // Broadcast movement to network:
                if (!network) network = FindObjectOfType<NetworkController>();
                Quaternion rot = transform.rotation;
                if (useLocalRotation)
                {
                    rot = transform.localRotation;
                } 
                network.BroadcastPlayerMovement(transform.position, rot);
                
                

                // Log movement:
                string movementInfo = "local player moved to P:" +
                    transform.position.ToString() + " R:" + rot.ToString();
                CCLogger.Log(CCLogger.EVENT_PLAYER_MOVE, movementInfo);

                // update local comparators
                lastPos = transform.position;
                lastRot = rot;
                lastSend = Time.time;
            }
            
        }
    }
}
