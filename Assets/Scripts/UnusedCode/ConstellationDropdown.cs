﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstellationDropdown : MonoBehaviour
{
    ConstellationsController constellationsController;
    TMP_Dropdown dropdown;
    private List<string> allConstellationNames;
    SimulationManager manager { get => SimulationManager.Instance; }

    private DataManager dataManager { get => DataManager.Instance; }

    void Start()
    {
        constellationsController = FindObjectOfType<ConstellationsController>();
        dropdown = GetComponent<TMP_Dropdown>();
        //Add listener for when the value of the Dropdown changes, to take action
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        string constellationName = change.captionText.text;
        CCDebug.Log("Selected constellation " + constellationName, LogLevel.Info, LogMessageCategory.Interaction);
        
        if (constellationsController)
        {
            if (constellationName.ToLower() == "all")
            {
                constellationsController.HighlightAllConstellations(true);
                manager.CurrentlySelectedStar = null;
                updateStarPanel(false);
            }
            else if (constellationName.ToLower() == "none")
            {
                constellationsController.HighlightAllConstellations(false);
                manager.CurrentlySelectedStar = null;
                updateStarPanel(false);
            }
            else
            {
                List<Star> allStarsInConstellation = dataManager.AllStarsInConstellationByFullName(constellationName);
                CCDebug.Log("Count of stars: " + allStarsInConstellation.Count, LogLevel.Info, LogMessageCategory.Interaction);
                if (allStarsInConstellation != null && allStarsInConstellation.Count > 0)
                {
                    Star brightestStar = allStarsInConstellation.OrderBy(s => s.Mag).FirstOrDefault();
                    CCDebug.Log(brightestStar, LogLevel.Info, LogMessageCategory.Interaction);
                    DataController dc = manager.DataControllerComponent;
                    StarComponent sc = dc.GetStarById(brightestStar.uniqueId);
                    manager.CurrentlySelectedStar = sc;
                    
                    // update legacy UI
                   updateStarPanel(true);
                    
                    // broadcast selection
                    InteractionController interactionController = FindObjectOfType<InteractionController>();
                    interactionController.ShowCelestialObjectInteraction(brightestStar.ProperName,
                        brightestStar.Constellation, brightestStar.uniqueId, true);
                }
                constellationsController.HighlightSingleConstellation(constellationName, manager.LocalPlayerColor);
            }
        }
    }

    void updateStarPanel(bool showPanel)
    {
        // update UI (legacy)
        MainUIController mainUIController = FindObjectOfType<MainUIController>();
        if (mainUIController != null)
        {
            if (!showPanel)
            {
                mainUIController.HidePanel("StarInfoPanel");
            }
            else
            {
                mainUIController.ShowPanel("StarInfoPanel");
                mainUIController.starInfoPanel.GetComponent<StarInfoPanel>().UpdateStarInfoPanel();
            }
        }
    }

    public void InitConstellationNames(List<string> constellationNames, string currentConstellation)
    {
        if (allConstellationNames == null || allConstellationNames.Count == 0)
        {
            allConstellationNames = constellationNames;
            allConstellationNames.Remove("");
            allConstellationNames.Sort();

            // Get dropdown reference in case InitConstellationNames is called before Start
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.AddOptions(allConstellationNames);
            int initialValue = allConstellationNames.IndexOf(currentConstellation);

            if (initialValue < 0)
            {
                initialValue = 0;
            }

            dropdown.value = initialValue;
        }
    }

    public void UpdateConstellationSelection(string currentConstellation)
    {
        // Get dropdown reference in case InitConstellationNames is called before Start
        dropdown = GetComponent<TMP_Dropdown>();
        List<TMP_Dropdown.OptionData> dropdownOptions = dropdown.options;
        int dropdownValue = dropdownOptions.FindIndex(el => el.text == currentConstellation);
        if (dropdownValue < 0)
        {
            dropdownValue = 0;
        }
        dropdown.SetValueWithoutNotify(dropdownValue);
    }
}
