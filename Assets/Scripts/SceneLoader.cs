using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject LoadingPanel;
    public GameObject ButtonPanel;
    public Slider slider;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            LoadScene(1);
        }
        else if (Input.GetKeyDown("2"))
        {
            LoadScene(2);
        }
        else if (Input.GetKeyDown("3"))
        {
            LoadScene(3);
        }
        else if (Input.GetKeyDown("4"))
        {
            LoadScene(4);
        }
    }
    public void LoadScene (int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    IEnumerator LoadAsynchronously (int sceneIndex)
    {
        ButtonPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;
            yield return null;
        }
    }
}
