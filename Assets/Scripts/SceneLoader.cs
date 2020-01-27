﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject regularCameraRig;
    [SerializeField]
    private GameObject vrCameraRig;
    [SerializeField]
    private GameObject vrEventSystem;
    [SerializeField]
    private GameObject defaultEventSystem;

    private GameObject vrCamera;
    public void Start()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        string model = UnityEngine.XR.XRDevice.model != null ? UnityEngine.XR.XRDevice.model : "";
        if (!string.IsNullOrEmpty(model))
        {
            // we have an XR device attached! Oculus Quest is detected as Rift S if it is connected with USB3 Oculus Link
            // TODO: If we detect an HTC Vive, Valve Index, or other headset, we need to do more work to use different controllers
            Debug.Log("Detected XR device: " + model);
            setupXRCameras();
        } 
        else
        {
            // assume no headset plugged in or available - this may be an Android AR build?
            setupStandardCameras();
        }
#else
       setupStandardCameras();
#endif

    }

    private void setupXRCameras()
    {
        GameObject existingCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (existingCamera != null)
        {
            existingCamera.SetActive(false);
        }
        vrCamera = Instantiate(vrCameraRig);

        string currentScene = SceneManager.GetActiveScene().name;

        Canvas[] allUICanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allUICanvases)
        {
            c.renderMode = RenderMode.WorldSpace;

            c.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
            c.planeDistance = 10;
            c.worldCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
            c.GetComponent<GraphicRaycaster>().enabled = false;
            if (c.GetComponent<OVRRaycaster>() == null) c.gameObject.AddComponent<OVRRaycaster>();
            c.GetComponent<OVRRaycaster>().enabled = true;
            
            
            if (c.gameObject.name == "MainUI")
            {
                c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 750);
                c.transform.position = new Vector3(3, 3, 5);
                if (currentScene == "EarthInteraction")
                {
                    c.transform.position = new Vector3(5, 1, 1);
                    c.transform.rotation = Quaternion.Euler(0, 30, 0);
                }

            }
            else if (c.gameObject.name == "NetworkUI")
            {
                c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);
                c.transform.position = new Vector3(-3, 3, 5);
            }
            else
            {
                c.transform.position = new Vector3(0, 3, 5);
            }
            
        }
        
        if (!defaultEventSystem) defaultEventSystem = GameObject.Find("EventSystem");
        if (defaultEventSystem) defaultEventSystem.SetActive(false);
        Instantiate(vrEventSystem);
        LaserPointer lp = FindObjectOfType<LaserPointer>();
        lp.laserBeamBehavior = LaserPointer.LaserBeamBehavior.OnWhenHitTarget;

        // some scene-specific pieces to remove
        GameObject horizonCamControls = GameObject.Find("HorizonCameraControls");
        if (horizonCamControls != null)
        {
            horizonCamControls.SetActive(false);
        }
        if (currentScene == "EarthInteraction" || currentScene == "Stars")
        {
            Camera vrCam = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
            vrCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            vrCam.GetComponent<Camera>().backgroundColor = Color.black;

        }
        if (currentScene == "Horizon")
        {
            vrCamera.transform.position = new Vector3(0, 2, 0);
        }
        GameObject avatar = GameObject.FindGameObjectWithTag("LocalPlayerAvatar");
        if (avatar != null)
        {
            avatar.SetActive(false);
        }
    }


    private void setupStandardCameras()
    {
        GameObject existingCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (existingCamera == null && regularCameraRig != null)
        {
            GameObject mainCam = new GameObject("MainCamera", typeof(Camera));
            mainCam.tag = "MainCamera";
            mainCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            mainCam.GetComponent<Camera>().backgroundColor = Color.black;
        }
    }
}
