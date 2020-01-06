using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    int firstSceneIndex = 0;
    string[] scenes;
    // Start is called before the first frame update
    void Start()
    {
        // simulation manager uses platform-dependent compilation to filter list of scenes
        scenes = SimulationManager.GetInstance().Scenes;

        string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0));
        if (sceneName == "LoadSim") firstSceneIndex = 1;
    }

    public void LoadPreviousScene ()
    {
        int currentIdx = Array.IndexOf(scenes, SceneManager.GetActiveScene().name);
        Debug.Log("LoadPreviousScene");
        if (currentIdx <= firstSceneIndex)
        {
            SceneManager.LoadScene(scenes[scenes.Length - 1]);
        }
        else
        {
            SceneManager.LoadScene(scenes[currentIdx - 1]);
        }
    }

    public void LoadNextScene ()
    {
        int currentIdx = Array.IndexOf(scenes, SceneManager.GetActiveScene().name);
        Debug.Log("LoadNextScene");
        if (currentIdx + 1 >= scenes.Length)
        {
            SceneManager.LoadScene(firstSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(scenes[currentIdx + 1]);
        }
    }
}
