﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GroupSelectionWizard : MonoBehaviour
{
    public GameObject ButtonPanel;
    public GameObject smallOrangeButtonPrefab;
    public GameObject NextButton;
    public TMPro.TextMeshProUGUI DirectionsText;
    public GameObject NextScreen;
   
    private List<Action> steps;
    private int currentStep;
    private List<GameObject> buttons;
 
    // The user record:
    private UserRecord userRecord;

    // Start is called before the first frame update
    private void Start()
    {
        userRecord = new UserRecord();
        currentStep = 0;
        steps = new List<Action>();
        steps.Add(ShowGroups);
        steps.Add(ShowColors);
        steps.Add(ShowAnimals);
        steps.Add(ShowNumbers);
        buttons = new List<GameObject>();
        ShowGroups();
        Button nextButton = NextButton.GetComponent<Button>();
        nextButton.onClick.AddListener(NextStep);
    }

    private Button addButton()
    {
        GameObject button = Instantiate(smallOrangeButtonPrefab);
        RectTransform buttonTransform = button.GetComponent<RectTransform>();
        buttonTransform.SetParent(ButtonPanel.transform);
        buttonTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        buttonTransform.localPosition = new Vector3(buttonTransform.localPosition.x, buttonTransform.localPosition.y, 0);

        return button.GetComponent<Button>();
    }

    public void ShowGroups()
    {
        SetTitleText("Pick a Group");
        foreach (string name in NetworkController.roomNames)
        {
            Button b = addButton();
            TMPro.TextMeshProUGUI label = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            buttons.Add(b.gameObject);
            if (label != null)
            {
                label.SetText(name);
            }
            if (b != null)
            {
                b.onClick.AddListener(() => HandleGroupClick(name));
            }
        }
    }

    public void ShowColors()
    {
        ClearAllButtons();
        SetTitleText("Pick a Color");
        int index = 0;
        foreach (string name in UserRecord.ColorNames)
        {
            Color color = UserRecord.ColorValues[index++];
            Button b = addButton();
            TMPro.TextMeshProUGUI label = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            ColorBlock c = b.colors;
            c.normalColor = color;
            c.highlightedColor = Color.Lerp(color, Color.white, 0.5f);
            c.pressedColor = Color.Lerp(color, Color.white, 0.5f);
            c.selectedColor = Color.Lerp(color, Color.black, 0.5f);
            b.colors = c;
            buttons.Add(b.gameObject);
            if (label != null)
            {
                label.SetText(name);
            }
            if (b != null)
            {
                b.onClick.AddListener(() => HandleColorClick(name, color));
            }
        }
    }


    public void ShowAnimals()
    { 
        ClearAllButtons();
        SetTitleText("Pick an animal name");
        foreach (string name in UserRecord.AnimalNames)
        {
            Button b = addButton();
            
            buttons.Add(b.gameObject);
            TMPro.TextMeshProUGUI label = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null)
            {
                label.SetText(name);
            }
            if (b != null)
            {
                b.onClick.AddListener(() => HandleAnimalClick(name));
            }
        }
    }

    public void ShowNumbers()
    {
        ClearAllButtons();
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        SetTitleText("Pick an number");
        foreach (int number in numbers)
        {
            string numberString = number.ToString();
           
            Button b = addButton();
            buttons.Add(b.gameObject);
            TMPro.TextMeshProUGUI label = b.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (label != null)
            {
                label.SetText(numberString);
            }
            if (b != null)
            {
                b.onClick.AddListener(() => HandleNumberClick(numberString));
            }
        }
    }

    public void ClearAllButtons()
    {
        foreach (GameObject o in buttons) {
            Destroy(o);
        }
        buttons.Clear();
    }


    public void HandleGroupClick(string name)
    {
        userRecord.group = name;
        DirectionsText.text = name;
        EnableNext();
    }

    
    public void HandleAnimalClick(string name)
    {
        userRecord.animal = name;
        DirectionsText.text = userRecord.Username;
        EnableNext();
    }

    public void HandleColorClick(string name, Color color)
    {
        userRecord.color = color;
        userRecord.colorName = name;
        DirectionsText.text = userRecord.Username;
        DirectionsText.color = color;
        EnableNext();
    }


    public void HandleNumberClick(string name)
    {
        userRecord.number = name;
        DirectionsText.text = userRecord.Username;
        EnableNext();
    }

    private void NextStep()
    {
        this.currentStep++;
        if(currentStep < steps.Count)
        {
            steps[currentStep]();
            DisableNext();
        } else
        {
            LaunchScene();
        }
    }

    private void LaunchScene()
    {
        Debug.Log("Launching ...");
        SimulationManager.GetInstance().LocalPlayer = userRecord;
        if(NextScreen)
        {
            ButtonPanel.SetActive(false);
            NextScreen.SetActive(true);
        }
    }

    private void EnableNext()
    {
        if(NextButton)
        {
            NextButton.SetActive(true);
        }
    }


    private void DisableNext()
    {
        if (NextButton)
        {
            NextButton.SetActive(false);
        }
    }
    private void SetTitleText(string newText) {
        if(DirectionsText)
        {
            DirectionsText.text = newText;
        }
    }
}
