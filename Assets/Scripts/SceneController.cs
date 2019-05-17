using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("4") || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LoadPreviousScene();
        }
        else if (Input.GetKeyDown("6") || Input.GetKeyDown(KeyCode.RightArrow))
        {
            LoadNextScene();
        }
    }

    public void LoadPreviousScene ()
    {
        Debug.Log("LoadPreviousScene");
        if (SceneManager.GetActiveScene().buildIndex <= 0)
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
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
