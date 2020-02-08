using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SimulationConstants;

public class PlayerMovement : MonoBehaviour
{
    private Vector3 lastPos;
    private Quaternion lastRot;
    private float lastSend = 0;
    NetworkController network;
    public bool useLocalRotation = false;
    string sceneName = "";
    private Transform cameraTransform;
    private Transform cameraParentTransform;
    private Vector3 lastCameraRotation;
    
    private SimulationManager manager
    {
        get { return SimulationManager.GetInstance(); }
    }
    private void Awake()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
        sceneName = SceneManager.GetActiveScene().name;
    }

    void FixedUpdate()
    {
        if (sceneName != SimulationConstants.SCENE_LOAD) {
            if (lastPos != transform.position || lastRot != transform.rotation)
            {
                // send update - no more frequently than once per second
                if (Time.time - manager.MovementSendInterval > lastSend)
                {
                    // Broadcast movement to network:
                    if (!network) network = FindObjectOfType<NetworkController>();
                    Quaternion rot = transform.rotation;
                    if (useLocalRotation)
                    {
                        rot = transform.localRotation;
                    }
                    
                    network.BroadcastPlayerMovement(transform.position, rot);
                    GetCameraRotationAndUpdatePin();
                    
                    // Log movement:
                    string movementInfo = "local player moved to P:" +
                        transform.position.ToString() + " R:" + rot.ToString();
                    CCLogger.Log(LOG_EVENT_PLAYER_MOVE, movementInfo);

                    // update local comparators
                    lastPos = transform.position;
                    lastRot = rot;
                    lastSend = Time.time;
                }
            }
            GetCameraRotationAndUpdatePin();
        }
    }

    public void GetCameraRotationAndUpdatePin()
    {
        if (sceneName == SimulationConstants.SCENE_HORIZON)
        {
#if !UNITY_ANDROID
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }
            if (cameraParentTransform == null)
            {
                cameraParentTransform = cameraTransform.parent;
            }
#else
                        if (cameraTransform == null)
                        {
                            cameraTransform = Camera.main.transform;
                        }
                        if (cameraParentTransform == null)
                        {
                            cameraParentTransform = cameraTransform;
                        }
#endif
            Vector3 cameraRotation = new Vector3(cameraTransform.rotation.eulerAngles.x, cameraTransform.rotation.eulerAngles.y, 0);
            if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON)
            {
                // clamp X rotation so users don't look at the floor
                cameraRotation.x = Mathf.Clamp(cameraTransform.rotation.eulerAngles.x, 0f, -90f);

            }
            if (manager.LocalPlayerLookDirection != cameraRotation)
            {
                if (Time.time - manager.MovementSendInterval > lastSend)
                {
                    CCDebug.Log("Sending updated pin", LogLevel.Verbose, LogMessageCategory.Networking);
                    manager.LocalPlayerLookDirection = cameraRotation;
                    
                    SimulationEvents.GetInstance().PushPinUpdated.Invoke(manager.LocalPlayerPin, manager.LocalPlayerLookDirection);
                    lastSend = Time.time;
                }
            }
        }
    }
}
