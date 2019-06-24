using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    int firstSceneIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0));
        if (sceneName == "LoadSim") firstSceneIndex = 1;
    }

    public void LoadPreviousScene ()
    {
        Debug.Log("LoadPreviousScene");
        if (SceneManager.GetActiveScene().buildIndex <= firstSceneIndex)
        {
            SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }

    public void LoadNextScene ()
    {
        Debug.Log("LoadNextScene");
        if (SceneManager.GetActiveScene().buildIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(firstSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
