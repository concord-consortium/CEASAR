using System;
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
        //steps.Add(ShowNetworks);
        buttons = new List<GameObject>();
        ShowGroups();
        Button nextButton = NextButton.GetComponent<Button>();
        nextButton.onClick.AddListener(NextStep);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void ShowGroups()
    {
        SetTitleText("Pick a Group");
        foreach (string name in NetworkController.roomNames)
        {
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(ButtonPanel.transform);
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();
            buttons.Add(button);
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
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(ButtonPanel.transform);
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();
            ColorBlock c = b.colors;
            c.normalColor = color;
            c.highlightedColor = Color.Lerp(color, Color.white, 0.5f);
            c.pressedColor = Color.Lerp(color, Color.white, 0.5f);
            c.selectedColor = Color.Lerp(color, Color.black, 0.5f);
            b.colors = c;
            buttons.Add(button);
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
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(ButtonPanel.transform);
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();
            
            buttons.Add(button);
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
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(ButtonPanel.transform);
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();

            buttons.Add(button);
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

    public void ShowNetworks()
    {
        ClearAllButtons();
        List<ServerRecord> servers = ServerList.List();
        SetTitleText("Connect to server:");
        foreach (ServerRecord server in servers)
        {
            string name = server.name;
            string address = server.address;
            GameObject button = Instantiate(smallOrangeButtonPrefab);
            button.GetComponent<RectTransform>().SetParent(ButtonPanel.transform);
            button.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            TMPro.TextMeshProUGUI label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            Button b = button.GetComponent<Button>();

            buttons.Add(button);
            if (label != null)
            {
                label.SetText(name);
            }
            if (b != null)
            {
                b.onClick.AddListener(() => HandleConnect(server));
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

    public void HandleConnect(ServerRecord server)
    {
        //networkController.roomName = studentRecord.group;
        string username = userRecord.Username;
        //networkController.ConnectToEndpoint(server.address);
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
            gameObject.SetActive(false);
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
