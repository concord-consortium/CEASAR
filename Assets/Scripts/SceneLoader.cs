using System.Collections;
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
    public void Start()
    {
#if UNITY_ANDROID
        GameObject existingCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (existingCamera != null)
        {
            existingCamera.SetActive(false);
        }
        Instantiate(vrCameraRig);
        Canvas[] mainUI = FindObjectsOfType<Canvas>();
        foreach (Canvas c in mainUI)
        {
            c.renderMode = RenderMode.WorldSpace;
            c.transform.position = new Vector3(0, 2, 5);
            c.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
            c.planeDistance = 10;
            c.worldCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
            c.GetComponent<GraphicRaycaster>().enabled = false;
            if (c.GetComponent<OVRRaycaster>() == null) c.gameObject.AddComponent<OVRRaycaster>();
            c.GetComponent<OVRRaycaster>().enabled = true;
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
        
#else
        if (regularCameraRig == null)
        {
            GameObject mainCam = new GameObject("MainCamera", typeof(Camera));
            mainCam.tag = "MainCamera";
            mainCam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            mainCam.GetComponent<Camera>().backgroundColor = Color.black;
        }
        else
        {
            Instantiate(regularCameraRig);
        }
#endif

    }
    
}
