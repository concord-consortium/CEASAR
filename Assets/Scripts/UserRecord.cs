
using UnityEngine;
using System.Collections.Generic;

public class UserRecord
{
    readonly private static int COLOR_INDEX = 0;
    readonly private static int ANIMAL_INDEX = 1;
    readonly private static int NUMBER_INDEX = 2;
    readonly private static int GROUP_INDEX = 3;
    readonly private static int NUM_FIELDS = 4;
    public const string PLAYER_PREFS_NAME_KEY = "CAESAR_USERNAME";

    // The username record items:
    public string group;
    public string animal;
    public string colorName;
    public Color color;
    public string number;

    public UserRecord()
    {
        LoadRandomValues();
        string username = PlayerPrefs.GetString(PLAYER_PREFS_NAME_KEY);
        if(username != null && username.Length > 0)
        {
            FromUsername(username);
        }
    }

    public UserRecord(string username)
    {
        FromUserName(username);
    }

    public void SaveToPrefs()
    {
        PlayerPrefs.SetString(PLAYER_PREFS_NAME_KEY, Username);
    }

    public void Randomize()
    {
        LoadRandomValues();
    }

    private void FromUsername(string username)
    {
        string[] values = username.Split(' ');
        string status = "Username parse error. Created new randomized user";
        if (values.Length >= NUM_FIELDS)
        {

            string _animal = values[ANIMAL_INDEX];
            string _group = values[GROUP_INDEX];
            string _color = values[COLOR_INDEX];
            string _number = values[NUMBER_INDEX];

            if (
                ColorNames.Contains(_color) &&
                AnimalNames.Contains(_animal) &&
                GroupNames.Contains(_group)
            )
            {
                animal = _animal;
                colorName = _color;
                color = GetColorForUsername(username);
                number = _number;
                group = _group;
                status = "New user parsed OK.";
            }
        }
        Debug.Log(status);
    }

    private void LoadRandomValues()
    {
        System.Random rng = new System.Random();

        int groupIndex = rng.Next(GroupNames.Count - 1);
        int colorIndex = rng.Next(ColorNames.Count - 1);
        int animalIndex = rng.Next(AnimalNames.Count - 1);

        group = NetworkController.roomNames[groupIndex];
        number = rng.Next(9).ToString();
        animal = AnimalNames[animalIndex];
        colorName = ColorNames[colorIndex];
        color = ColorValues[colorIndex];
    }

    public string Username
    {
        get { return $"{colorName} {animal} {number} {group}"; }
    }


    /**************************** Static Methods *****************************/

    public static List<string> GroupNames { get; } = new List<string>(NetworkController.roomNames);

    private static List<string> colorNames;
    public static List<string> ColorNames
    {
        get
        {
            if(!ResourcesLoaded)
            {
                LoadTextResources();
            }
            return colorNames;
        }
    }

    private static List<string> animalNames;
    public static List<string> AnimalNames
    {
        get
        {
            if (!ResourcesLoaded)
            {
                LoadTextResources();
            }
            return animalNames;
        }
    }

    private static List<Color> colorValues;
    public static List<Color> ColorValues
    {
        get
        {
            if (!ResourcesLoaded)
            {
                LoadTextResources();
            }
            return colorValues;
        }
    }

    private static bool ResourcesLoaded = false;
    private static void LoadTextResources()
    {
        TextAsset colorList = Resources.Load("colors-new") as TextAsset;
        TextAsset animalList = Resources.Load("animals-new") as TextAsset;
        char[] lineDelim = new char[] { '\r', '\n' };
        string[] colorsFull = colorList.text.Split(lineDelim, System.StringSplitOptions.RemoveEmptyEntries);
        colorNames = new List<string>();
        colorValues = new List<Color>();
        foreach (string c in colorsFull)
        {
            colorNames.Add(c.Split(':')[0]);
            Color color;
            ColorUtility.TryParseHtmlString(c.Split(':')[1], out color);
            colorValues.Add(color);
        }
        animalNames = new List<string>(animalList.text.Split(lineDelim, System.StringSplitOptions.RemoveEmptyEntries));
        ResourcesLoaded = true;
    }

    public static string GetColorNameForUsername(string username)
    {
        return username.Split(' ')[0];
    }

    public static Color GetColorForUsername(string username)
    {
        string colorName = GetColorNameForUsername(username);
        if (ColorNames.Contains(colorName))
        {
            return ColorValues[ColorNames.IndexOf(colorName)];
        }
        else
        {
            Debug.Log("Color not found for " + colorName + " as part of " + username);
            return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.9f, 1f);
        }
    }

    public static string UsernameFromPrefs()
    {
        return PlayerPrefs.GetString(PLAYER_PREFS_NAME_KEY);
    }

    public static UserRecord FromUserName(string username)
    {
        return new UserRecord(username);
    }

}


