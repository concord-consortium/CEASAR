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
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        string model = UnityEngine.XR.XRDevice.model != null ? UnityEngine.XR.XRDevice.model : "";
        if (!string.IsNullOrEmpty(model))
        {
            // different scenes for Quest
            playableScenes = SimulationConstants.SCENES_PLAYABLE_VR;
        }
#endif
        if (playableScenes.Length > 2)
        {
            foreach (string sceneName in playableScenes)
            {
                if (currentSceneName != sceneName)
                {
                    UpdateButton(buttonCounter, sceneName);
                    buttonCounter++;
                }
            }
        }
        else
        {
            for (var i = 0; i < playableScenes.Length; i++)
            {
                UpdateButton(i, playableScenes[i]);
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

        Image buttonBackgroundImage = button.gameObject.GetComponent<Image>();
        if (SceneManager.GetActiveScene().name.StartsWith(sceneName))
        {
            button.enabled = false;
            buttonBackgroundImage.color = Color.gray;
        } else
        {
            button.enabled = true;
            buttonBackgroundImage.color = Color.white;
        }
    }

    private string sceneNameForDisplay(string sceneName)
    {
        // EarthInteraction was too long:
        if (sceneName == SimulationConstants.SCENE_EARTH) return "Earth";

        // Other scene names are fine for now.
        return sceneName;
    }
}
