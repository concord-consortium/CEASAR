using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



class StudentRecord
{
    // The username record items:
    public string group;
    public string animal;
    public string color;
    public string number;

    public DateTime startTime;

    
    public StudentRecord()
    {
        group = animal = color = number = "";
        startTime = DateTime.UtcNow;
    }

    public string Username()
    {
        return $"{color}-{animal}-{number}-{group}";
    }

}

public class GroupSelectionWizard : MonoBehaviour
{
    public GameObject ButtonPanel;
    public GameObject smallOrangeButtonPrefab;
    public GameObject NextButton;
    public TMPro.TextMeshProUGUI DirectionsText;
    public GameObject NextScreen;
    public NetworkController networkController;

    private List<Action> steps;
    private int currentStep;
    private List<GameObject> buttons;
    

    // Names and colors for groups:
    private List<string> ColorNames;
    private List<string> AnimalNames;
    private List<Color> ColorValues;

    // The username record:
    private StudentRecord studentRecord;

    // Start is called before the first frame update
    private void Start()
    {
        studentRecord = new StudentRecord();
        LoadTextResources();
        currentStep = 0;
        steps = new List<Action>();
        steps.Add(ShowGroups);
        steps.Add(ShowColors);
        steps.Add(ShowAnimals);
        steps.Add(ShowNumbers);
        steps.Add(ShowNetworks);
        buttons = new List<GameObject>();
        ShowGroups();
        Button nextButton = NextButton.GetComponent<Button>();
        nextButton.onClick.AddListener(NextStep);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LoadTextResources()
    {
        TextAsset colorList = Resources.Load("colors-new") as TextAsset;
        TextAsset animalList = Resources.Load("animals-new") as TextAsset;
        char[] lineDelim = new char[] { '\r', '\n' };
        string[] colorsFull = colorList.text.Split(lineDelim, StringSplitOptions.RemoveEmptyEntries);
        List<string> colors = new List<string>();
        List<Color> colorValues = new List<Color>();
        foreach (string c in colorsFull)
        {
            colors.Add(c.Split(':')[0]);
            Color color;
            ColorUtility.TryParseHtmlString(c.Split(':')[1], out color);
            colorValues.Add(color);
        }
        ColorNames = colors;
        ColorValues = colorValues;
        AnimalNames = new List<string>(animalList.text.Split(lineDelim, StringSplitOptions.RemoveEmptyEntries));
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
        foreach (string name in ColorNames)
        {
            Color color = ColorValues[index++];
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
        foreach (string name in AnimalNames)
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
        studentRecord.group = name;
        networkController.roomName = studentRecord.Username();
        DirectionsText.text = name;
        EnableNext();
    }

    
    public void HandleAnimalClick(string name)
    {
        studentRecord.animal = name;
        DirectionsText.text = studentRecord.Username();
        EnableNext();
    }

    public void HandleColorClick(string name, Color color)
    {
        studentRecord.color = name;
        DirectionsText.text = studentRecord.Username();
        DirectionsText.color = color;
        EnableNext();
    }


    public void HandleNumberClick(string name)
    {
        studentRecord.number = name;
        DirectionsText.text = studentRecord.Username();
        EnableNext();
    }

    public void HandleConnect(ServerRecord server)
    {
        networkController.roomName = studentRecord.group;
        Debug.Log(networkController.roomName);
        string username = studentRecord.Username();
        networkController.SetUsername(username);
        networkController.ConnectToEndpoint(server.address);
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
