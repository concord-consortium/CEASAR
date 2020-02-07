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
    public GameObject FastLoginButton;
    public TMPro.TextMeshProUGUI DirectionsText;
    public GameObject GroupLabel;
    public GameObject NextScreen;
    public GameObject Restart;

    private List<Action> steps;
    private int currentStep;
    private List<GameObject> buttons;
 
    // The user record:
    private UserRecord userRecord;
    
    private SimulationManager manager { get { return SimulationManager.GetInstance(); }}

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
        Button restartButton = Restart.GetComponent<Button>();
        restartButton.onClick.AddListener(restart);
        ShowFastLogin();
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

    private void ShowFastLogin()
    {
        if (UserRecord.PlayerHasValidPrefs())
        {
            FastLoginButton.SetActive(true);
            Button b = FastLoginButton.GetComponent<Button>();
            TMPro.TextMeshProUGUI tgui = FastLoginButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            tgui.text = $"{userRecord.Username} {userRecord.group}";
            
            if (!string.IsNullOrEmpty(userRecord.group))
            {
                TMPro.TextMeshProUGUI textMesh = GroupLabel.GetComponent<TMPro.TextMeshProUGUI>();
                textMesh.text = userRecord.group;
            }
            
            b.onClick.AddListener(LaunchScene);
        }
        else
        {
            DisableFastLogin();
        }
    }

    private void DisableFastLogin()
    {
        FastLoginButton.SetActive(false);
    }

    private void ShowGroups()
    {
        SetTitleText("Pick a Group");
        foreach (string name in UserRecord.GroupNames)
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

    private void ShowColors()
    {
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


    private void ShowAnimals()
    { 
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

    private void ShowNumbers()
    {
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

    private void ClearAllButtons()
    {
        foreach (GameObject o in buttons) {
            Destroy(o);
        }
        buttons.Clear();
    }


    private void HandleGroupClick(string name)
    {
        userRecord.group = name;
        
        if (GroupLabel)
        {
            TMPro.TextMeshProUGUI textMesh = GroupLabel.GetComponent<TMPro.TextMeshProUGUI>();
            GroupLabel.SetActive(true);
            textMesh.text = userRecord.group;
        }
        SetTitleText(name);
        EnableNext();
    }


    private void HandleAnimalClick(string name)
    {
        userRecord.animal = name;
        SetTitleText(name);
        EnableNext();
    }

    private void HandleColorClick(string name, Color color)
    {
        userRecord.color = color;
        userRecord.colorName = name;
        SetTitleText(name);
        DirectionsText.color = color;
        EnableNext();
    }


    private void HandleNumberClick(string number)
    {
        userRecord.number = number;
        SetTitleText(number);
        EnableNext();
    }

    private void NextStep()
    {
        currentStep++;
        DisableNext();
        DisableFastLogin(); // If we are partially changed, its no good.
        if (currentStep < steps.Count)
        {
            ClearAllButtons();
            steps[currentStep]();
            EnableRestart();
        } else
        {
            LaunchScene();
        }
    }


    private void restart()
    {
        currentStep = 0;
        GroupLabel.SetActive(false);
        DisableRestart();
        DisableNext();
        ClearAllButtons();
        steps[currentStep]();
    }

    private Pushpin setLocationForGroup(string groupName)
    {
        Pushpin selectedGroupPin = UserRecord.GroupPins[groupName];
        manager.CrashSiteForGroup = selectedGroupPin;
        manager.CurrentLatLng = selectedGroupPin.Location;
        manager.CurrentSimulationTime = selectedGroupPin.SelectedDateTime;
        // manager.LocalUserPin = selectedGroupPin;
        Debug.Log("User selected group " + groupName + " " + selectedGroupPin);

        return selectedGroupPin;
    }
    private void LaunchScene()
    {
        DisableNext();
        DisableRestart();
        DisableFastLogin();
        DirectionsText.color = userRecord.color;
        Pushpin pin = setLocationForGroup(userRecord.group);
        manager.LocalPlayer = new Player(userRecord, pin);
        // generate starting look direction
        // use group + day of year as a seed
        System.Random rnd = new System.Random(DateTime.UtcNow.DayOfYear + UserRecord.GroupNames.IndexOf(userRecord.group));
        float lookDirection = rnd.Next(0, 360);
        manager.LocalPlayerLookDirection = new Vector3(0, lookDirection, 0);
        userRecord.SaveToPrefs();
        
        if(NextScreen)
        {
            SetTitleText($"{userRecord.Username} in {userRecord.group}");
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

    private void DisableRestart()
    {
        if (Restart)
        {
            Restart.SetActive(false);
        }
    }

    private void EnableRestart()
    {
        if (Restart)
        {
            Restart.SetActive(true);
        }
    }

    private void SetTitleText(string newText) {
        if(DirectionsText)
        {
            DirectionsText.text = newText;
        }
    }

  
}
