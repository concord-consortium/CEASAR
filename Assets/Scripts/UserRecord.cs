
using System;
using UnityEngine;
using System.Collections.Generic;

public class UserRecord
{
    public static bool operator ==(UserRecord p1, UserRecord p2) 
    {
        return p1.Username.Equals(p2.Username);
    }

    public static bool operator !=(UserRecord p1, UserRecord p2) 
    {
        return !p1.Username.Equals(p2.Username);
    }
    
    readonly private static int COLOR_INDEX = 0;
    readonly private static int ANIMAL_INDEX = 1;
    readonly private static int NUMBER_INDEX = 2;
    readonly private static int GROUP_INDEX = 3;
    readonly private static int NUM_FIELDS = 4;
    
    public const string DEFAULT_USER_COLOR = "red";
    // The username record items:
    public string group;
    public string animal;
    public string colorName;
    public Color color;
    public string number;

    public UserRecord()
    {
        LoadRandomValues();
        string username = PlayerPrefs.GetString(SimulationConstants.USERNAME_PREF_KEY);
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
        PlayerPrefs.SetString(SimulationConstants.USERNAME_PREF_KEY, Username);
        PlayerPrefs.SetString(SimulationConstants.USER_GROUP_PREF_KEY, group);
    }

    public void Randomize()
    {
        string oldGroup = group;
        LoadRandomValues();
        if (oldGroup != null)
        {   // Retain previous group name by default.
            group = oldGroup;
        }
    }

    private void FromUsername(string username)
    {
        if (UsernameIsValid(username))
        {
            colorName = UsernameListItem(username, ColorNames);
            color = ColorForColorName(colorName);
            group = PlayerPrefs.GetString(SimulationConstants.USER_GROUP_PREF_KEY);
            animal = UsernameListItem(username, AnimalNames);
            number = UsernameListItem(username, Numbers);
        }
    }

    private void LoadRandomValues()
    {
        System.Random rng = new System.Random();

        int groupIndex = rng.Next(GroupNames.Count - 1);
        int colorIndex = rng.Next(ColorNames.Count - 1);
        int animalIndex = rng.Next(AnimalNames.Count - 1);

        group = GroupNames[groupIndex];
        number = rng.Next(9).ToString();
        animal = AnimalNames[animalIndex];
        colorName = ColorNames[colorIndex];
        color = ColorValues[colorIndex];
    }

    public string Username
    {
        get { return $"{colorName.FirstCharToUpper()}{animal.FirstCharToUpper()}{number}"; }
    }


    /**************************** Static Methods *****************************/

    private static List<string> groupNames = new List<string>();

    public static List<string> GroupNames
    {
        get
        {
            if (!ResourcesLoaded)
            {
                LoadTextResources();
            }
            return groupNames;
        }
    }

    private static Dictionary<string, Pushpin> groupPins = new Dictionary<string, Pushpin>();

    public static Dictionary<string, Pushpin> GroupPins
    {
        get
        {
            if (!ResourcesLoaded)
            {
                LoadTextResources();
            }
            return groupPins;
        }
    }
    
    private static List<string> Numbers = new List<string>("1,2,3,4,5,6,7,8,9".Split(','));
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

        TextAsset groupList = Resources.Load("groups") as TextAsset;
        JSONObject groupsObject = JSONObject.Create(groupList.text);
        
        groupNames = groupsObject.keys;
        foreach (var g in groupsObject.keys)
        {
            LatLng loc = new LatLng
            {
                Latitude = groupsObject[g].GetField("latitude").n, 
                Longitude = groupsObject[g].GetField("longitude").n
            };
            DateTime crashDt = DateTime.Parse(groupsObject[g].GetField("crashdatetime").str);
            string locationName = $"Crash site {g.FirstCharToUpper()}";
            Pushpin p = new Pushpin(crashDt, loc, locationName);
            groupPins.Add(g, p);
        }

        ResourcesLoaded = true;
    }
    

    public static string GetColorNameForUsername(string username)
    {
        foreach (string colorName in colorNames)
        {
            if (username.IndexOf(colorName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return colorName;
            }
        }
        return DEFAULT_USER_COLOR;
    }

    public static Color ColorForColorName(string colorName)
    {
        return ColorValues[ColorNames.IndexOf(colorName)];
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
        return PlayerPrefs.GetString(SimulationConstants.USERNAME_PREF_KEY);
    }

    public static string UserGroupFromPrefs()
    {
        return PlayerPrefs.GetString(SimulationConstants.USER_GROUP_PREF_KEY);
    }

    private static string UsernameListItem(string username, List<string> list)
    {
        foreach (string item in list)
        {
            if (username.IndexOf(item, System.StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                return item;
            }
        }
        return null;
    }

    private static bool UsernameContainsListItem(string username, List<string> list)
    {
        string match = UsernameListItem(username, list);
        if (match != null && match.Length > 0) return true;
        return false;
    }

    public static bool UsernameContainsColor(string username) {
        return UsernameContainsListItem(username, ColorNames);
    }

    public static bool UsernameContainsAnimal(string username)
    {
        return UsernameContainsListItem(username, AnimalNames);
    }

    public static bool UsernameContainsNumber(string username)
    {
        return UsernameContainsListItem(username, Numbers);
    }

    public static bool UsernameIsValid(string username)
    {
        return UsernameContainsColor(username) &&
            UsernameContainsAnimal(username) &&
            UsernameContainsNumber(username);
    }

    public static bool PlayerHasValidPrefs()
    {
        string username = UsernameFromPrefs();
        string group = UserGroupFromPrefs();
        return (username != null) &&
            (group != null) &&
            UsernameIsValid(username);
    }

    public static UserRecord FromUserName(string username)
    {
        return new UserRecord(username);
    }

}


