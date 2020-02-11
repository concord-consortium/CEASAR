using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{    
    void OnEnable()
    {
        UpdateButtons(SceneManager.GetActiveScene().name);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateButtons(scene.name);
    }

    public void LoadNamedScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void UpdateButtons(string currentSceneName) {
        int buttonCounter = 0;
        string[] playableScenes = SimulationConstants.SCENES_PLAYABLE;
        foreach(string sceneName in playableScenes)
        {
            if(currentSceneName != sceneName)
            {
                UpdateButton(buttonCounter, sceneName);
                buttonCounter++;
            }
        }
    }

    public void UpdateButton(int buttonIndex, string sceneName)
    {
        Button[] buttons = this.GetComponentsInChildren<Button>();
        Button button = buttons[buttonIndex];

        // Add the event handler:
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => LoadNamedScene(sceneName));

        // Change the text label:
        TMPro.TextMeshProUGUI label = button.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        label.text = sceneNameForDisplay(sceneName);
    }

    private string sceneNameForDisplay(string sceneName)
    {
        // EarthInteraction was too long:
        if (sceneName == SimulationConstants.SCENE_EARTH) return "Earth";

        // Other scene names are fine for now.
        return sceneName;
    }
}
