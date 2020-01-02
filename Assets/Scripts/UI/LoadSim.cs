using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSim : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject ButtonPanel;
    public Slider slider;
    string[] scenes;
    // Start is called before the first frame update
    void Start()
    {
        // simulation manager uses platform-dependent compilation to filter list of scenes
        scenes = SimulationManager.GetInstance().Scenes;
    }
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(scenes[sceneIndex]));
    }
    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
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
