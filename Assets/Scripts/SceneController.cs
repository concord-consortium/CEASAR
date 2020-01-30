using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{    
    void Start()
    {
        UpdateButtons();
    }

    public void LoadNamedScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void UpdateButtons() {
        int buttonCounter = 0;
        string thisScene = SceneManager.GetActiveScene().name;
        string[] playableScenes = SimulationConstants.SCENES_PLAYABLE;
        foreach(string sceneName in playableScenes)
        {
            if(thisScene != sceneName)
            {
                UpdateButton(buttonCounter, sceneName);
                buttonCounter++;
            }
        }
    }

    public void UpdateButton(int buttonIndex, string name)
    {
        Button[] buttons = this.GetComponentsInChildren<Button>();
        Button button = buttons[buttonIndex];

        // Add the event handler:
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => LoadNamedScene(name));

        // Change the text label:
        TMPro.TextMeshProUGUI label = button.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        label.text = SceneNameForDispaly(name);
    }

    private string SceneNameForDispaly(string name)
    {
        // EarthInteraction was too long:
        if (name == SimulationConstants.SCENE_EARTH) return "Earth";

        // Other scene names are fine for now.
        return name;
    }
}
