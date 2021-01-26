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

    private GameObject cameraGameObject;
    private GameObject cameraControlUI;
    private string currentSceneName;
    private Camera currentCamera;

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
        currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == SimulationConstants.SCENE_LOAD)
        {
            SetupCameras();
        }
    }

    public void SetupCameras()
    {
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
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
            if ((device.name.ToLower().Contains("oculus") || device.name.ToLower().Contains("quest")) && device.role.ToString().ToLower() == "generic")
            {
                // we have an XR device attached! Oculus Quest is detected as Rift S if it is connected with USB3 Oculus Link
                // TODO: If we detect an HTC Vive, Valve Index, or other headset, we need to do more work to use different controllers
                CCDebug.Log("Setting up XR device: " + device.name, LogLevel.Info, LogMessageCategory.All);
                setupVRCamera();
                setupVREventSystem();
                hideOnScreenCameraControl();
                hideAvatars();
            }
            else
            {
             CCDebug.Log("Unknown device: " + device.name, LogLevel.Info, LogMessageCategory.All);
            }
#elif UNITY_WSA_10_0

            // setupXRCamera();
            // setupWorldSpaceUI();
            // setupMRTKEventSystem();
            // hideOnScreenCameraControl();
            hideAvatars();
#else
        setupStandardCameras();
#endif
        }
    }

    private void setupXRCamera()
    {
        cameraGameObject = GameObject.FindGameObjectWithTag("MainCamera");
        cameraGameObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cameraGameObject.GetComponent<Camera>().backgroundColor = Color.black;
    }

    private void setupVRCamera()
    {
        GameObject existingCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (existingCamera != null)
        {
            existingCamera.SetActive(false);
        }
        cameraGameObject = Instantiate(vrCameraRig);
        currentCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        if (currentSceneName == SimulationConstants.SCENE_EARTH || currentSceneName == SimulationConstants.SCENE_STARS)
        {
            currentCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            currentCamera.GetComponent<Camera>().backgroundColor = Color.black;
        }
        if (currentSceneName == SimulationConstants.SCENE_HORIZON)
        {
            cameraGameObject.transform.position = new Vector3(0, 2, 0);
            cameraGameObject.transform.rotation = Quaternion.Euler(0, SimulationManager.Instance.LocalPlayerLookDirection.y, 0);
        }
        if (currentSceneName == SimulationConstants.SCENE_STARS)
        {
            cameraGameObject.transform.position = new Vector3(0, 0, 0);
        }
        DefaultSceneCameraLayers = currentCamera.GetComponent<Camera>().cullingMask;
        // when webgl is the target, the LaserPointer component does not exist
#if !UNITY_WEBGL && !UNITY_WSA_10_0
        LaserPointer lp = FindObjectOfType<LaserPointer>();
        lp.laserBeamBehavior = LaserPointer.LaserBeamBehavior.OnWhenHitTarget;
#endif
    }
    private void setupWorldSpaceUI()
    {
        Canvas[] allUICanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas c in allUICanvases)
        {
            if (c.CompareTag("WorldUI"))
            {
                c.renderMode = RenderMode.WorldSpace;
                c.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
                c.planeDistance = 10;
#if UNITY_WSA_10_0
                // c.gameObject.AddComponent<NearInteractionTouchableUnityUI>();
                c.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);
                c.planeDistance = 1;
#endif

                c.worldCamera = currentCamera;
                c.GetComponent<GraphicRaycaster>().enabled = false;
#if !(UNITY_WEBGL || UNITY_WSA_10_0)
                if (c.GetComponent<OVRRaycaster>() == null) c.gameObject.AddComponent<OVRRaycaster>();
                c.GetComponent<OVRRaycaster>().enabled = true;
#endif
                if (c.CompareTag("MainUI"))
                {
                    //c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(440, 750);
                    c.transform.position = new Vector3(3, 4, 5);
#if UNITY_WSA_10_0
                    c.transform.position = new Vector3(0.4f, 0.5f, 1.2f);
#endif

                    if (currentSceneName == SimulationConstants.SCENE_EARTH)
                    {
                        c.transform.position = new Vector3(6, 2, 1);
                        c.transform.rotation = Quaternion.Euler(0, 30, 0);
                    }

                    if (currentSceneName == SimulationConstants.SCENE_HORIZON)
                    {
                        c.transform.rotation = Quaternion.Euler(SimulationManager.Instance.LocalPlayerLookDirection);
                    }
                    if (currentSceneName == SimulationConstants.SCENE_LOAD)
                    {
                        c.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    }

                }
                else if (c.gameObject.name == "NetworkUI")
                {
                    c.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 600);
                    c.transform.position = new Vector3(-3, 3, 5);
                }
                else
                {
                    c.transform.position = new Vector3(0, 1, 1);
                }
            }
        }
    }

    private void setupVREventSystem()
    {
        Camera vrCam = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();

        if (!defaultEventSystem) defaultEventSystem = GameObject.Find("EventSystem");
        if (defaultEventSystem) defaultEventSystem.SetActive(false);
        Instantiate(vrEventSystem);
    }

    private void setupMRTKEventSystem()
    {
        if (!defaultEventSystem) defaultEventSystem = GameObject.Find("EventSystem");
        if (defaultEventSystem) defaultEventSystem.SetActive(false);
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
            CCDebug.Log($"Setting rotation to {SimulationManager.Instance.LocalPlayerLookDirection}");
            cam.transform.rotation = Quaternion.Euler(SimulationManager.Instance.LocalPlayerLookDirection);
        }

        DefaultSceneCameraLayers = cam.GetComponent<Camera>().cullingMask;
    }
    private void hideOnScreenCameraControl()
    {
        // some scene-specific pieces to remove
        cameraControlUI = GameObject.Find("CameraControlUI");
        if (cameraControlUI != null)
        {
            cameraControlUI.SetActive(false);
        }
    }
    private void hideAvatars()
    {
        GameObject avatar = GameObject.FindGameObjectWithTag("LocalPlayerAvatar");
        if (avatar != null)
        {
            avatar.SetActive(false);
        }
    }

}