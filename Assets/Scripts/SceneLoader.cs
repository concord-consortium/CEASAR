using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
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
    
    private GameObject cameraControlUI;

    public LayerMask DefaultSceneCameraLayers;

    public LogLevel LogLevel = LogLevel.Info;
    public LogMessageCategory[] LogCategories;
    private void Start()
    {
        
        CCDebug.CurrentLevel = LogLevel;

        if (LogCategories != null && LogCategories.Length > 0)
        {
            CCDebug.Categories = LogCategories;
        }
        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_LOAD)
        {
            SetupCameras();
        }
    }
    public void SetupCameras()
    {
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);
        if (inputDevices.Count == 0)
        {
            CCDebug.Log("No VR / XR devices detected");
            // assume no headset plugged in or available - this may be an Android AR build?
            setupStandardCameras();
        }
        foreach (var device in inputDevices)
        {
            CCDebug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()), LogLevel.Info, LogMessageCategory.All);
            if ((device.name.ToLower().Contains("oculus") || device.name.ToLower().Contains("quest")) && device.role.ToString().ToLower() == "generic")
            {
                // we have an XR device attached! Oculus Quest is detected as Rift S if it is connected with USB3 Oculus Link
                // TODO: If we detect an HTC Vive, Valve Index, or other headset, we need to do more work to use different controllers
                CCDebug.Log("Setting up XR device: " + device.name, LogLevel.Info, LogMessageCategory.All);
                setupXRCameras();
            }
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
            if (c.gameObject.transform.tag != "WorldUI")
            {

                c.renderMode = RenderMode.WorldSpace;

                c.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
                c.planeDistance = 10;
                c.worldCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
                c.GetComponent<GraphicRaycaster>().enabled = false;
#if !UNITY_WEBGL
                if (c.GetComponent<OVRRaycaster>() == null) c.gameObject.AddComponent<OVRRaycaster>();
                c.GetComponent<OVRRaycaster>().enabled = true;
#endif

                if (c.gameObject.name == "MainUI")
                {
                    c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 750);
                    c.transform.position = new Vector3(3, 4, 5);
                    
                    if (currentScene == SimulationConstants.SCENE_EARTH)
                    {
                        c.transform.position = new Vector3(6, 2, 1);
                        c.transform.rotation = Quaternion.Euler(0, 30, 0);
                    }

                    if (currentScene == SimulationConstants.SCENE_HORIZON)
                    {
                        c.transform.rotation = Quaternion.Euler(SimulationManager.GetInstance().LocalPlayerLookDirection);
                    }

                }
                else if (c.gameObject.name == "NetworkUI")
                {
                    c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);
                    c.transform.position = new Vector3(-3, 3, 5);
                }
                else
                {
                    c.transform.position = new Vector3(0, 1, 4);
                }
            }
        }
        Camera vrCam = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        
        if (!defaultEventSystem) defaultEventSystem = GameObject.Find("EventSystem");
        if (defaultEventSystem) defaultEventSystem.SetActive(false);
        Instantiate(vrEventSystem);
#if !UNITY_WEBGL
        LaserPointer lp = FindObjectOfType<LaserPointer>();
        lp.laserBeamBehavior = LaserPointer.LaserBeamBehavior.OnWhenHitTarget;
        
#endif
        // some scene-specific pieces to remove
        cameraControlUI = GameObject.Find("CameraControlUI");
        if (cameraControlUI != null)
        {
            cameraControlUI.SetActive(false);
        }
        if (currentScene == SimulationConstants.SCENE_EARTH || currentScene == SimulationConstants.SCENE_STARS)
        {
            vrCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            vrCam.GetComponent<Camera>().backgroundColor = Color.black;

        }
        if (currentScene == SimulationConstants.SCENE_HORIZON)
        {
            vrCamera.transform.position = new Vector3(0, 2, 0);
            vrCamera.transform.rotation = Quaternion.Euler(0, SimulationManager.GetInstance().LocalPlayerLookDirection.y, 0);
        }
        if (currentScene == SimulationConstants.SCENE_STARS)
        {
            vrCamera.transform.position = new Vector3(0, 0, 0);
        }
        GameObject avatar = GameObject.FindGameObjectWithTag("LocalPlayerAvatar");
        if (avatar != null)
        {
            avatar.SetActive(false);
        }

        DefaultSceneCameraLayers = vrCam.GetComponent<Camera>().cullingMask;
    }


    private void setupStandardCameras()
    {
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        if (cam == null && regularCameraRig != null)
        {
            cam = new GameObject("MainCamera", typeof(Camera));
            cam.tag = "MainCamera";
            cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            cam.GetComponent<Camera>().backgroundColor = Color.black;
        }
        if (cameraControlUI == null) cameraControlUI = GameObject.Find("CameraControlUI");
        if (cameraControlUI != null)
        {
            bool orbitControlMode = SceneManager.GetActiveScene().name == SimulationConstants.SCENE_EARTH;
            cameraControlUI.GetComponent<UIControlCamera>().enableControls = true;
            cameraControlUI.GetComponent<UIControlCamera>().cameraContainer = cam.transform;
            cameraControlUI.GetComponent<UIControlCamera>().OrbitControlMode = orbitControlMode;
        }

        if (SceneManager.GetActiveScene().name == SimulationConstants.SCENE_HORIZON)
        {
            CCDebug.Log($"Setting rotation to {SimulationManager.GetInstance().LocalPlayerLookDirection}");
            cam.transform.rotation = Quaternion.Euler(SimulationManager.GetInstance().LocalPlayerLookDirection);
        }
        
        DefaultSceneCameraLayers = cam.GetComponent<Camera>().cullingMask;
    }
}
