using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    string[] scenes;
   
    public GameObject LoadingPanel;
    public GameObject ButtonPanel;
    public Slider slider;
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
        Instantiate(vrCameraRig);
        Canvas mainUI = FindObjectOfType<Canvas>();
        mainUI.renderMode = RenderMode.WorldSpace;
        mainUI.transform.position = new Vector3(0, 0, 5);
        mainUI.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        mainUI.planeDistance = 10;
        mainUI.worldCamera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
        mainUI.GetComponent<GraphicRaycaster>().enabled = false;
        mainUI.GetComponent<OVRRaycaster>().enabled = true;
        Destroy(defaultEventSystem);
        Instantiate(vrEventSystem);
        LaserPointer lp = FindObjectOfType<LaserPointer>();
        lp.laserBeamBehavior = LaserPointer.LaserBeamBehavior.OnWhenHitTarget;

        
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
        // simulation manager uses platform-dependent compilation to filter list of scenes
        scenes = SimulationManager.GetInstance().Scenes;
    }
    public void LoadScene (int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(scenes[sceneIndex]));
    }
    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously (string sceneName)
    {
        ButtonPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            yield return null;
        }
    }
}
