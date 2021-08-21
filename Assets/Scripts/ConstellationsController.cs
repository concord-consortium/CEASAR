using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstellationsController : MonoBehaviour
{

    SimulationManager manager { get => SimulationManager.Instance; }
    List<Constellation> constellations = new List<Constellation>();
    float lineWidth = .035f;
    bool sceneShouldShowConstellations = false;

    public bool SceneShouldShowConstellations
    {
        get => sceneShouldShowConstellations;
    }

    private StarComponent hoveredStar;

    public StarComponent HoveredStar
    {
        get { return hoveredStar; }
        set { hoveredStar = value; }
    }

    public void SetSceneParameters(float lineWidth, bool show)
    {
        this.lineWidth = lineWidth;
        this.sceneShouldShowConstellations = show;
        ShowAllConstellations(show);
    }

    public void AddConstellation(Constellation constellation)
    {
        constellations.Add(constellation);
    }

    public Constellation GetConstellation(string cFullName)
    {
        int index = constellations.FindIndex(el => el.constellationNameAbbr == cFullName);
        if (index < 0)
        {
            return constellations[index];
        }
        else
        {
            return null;
        }
    }

    public void HighlightSingleConstellation(string cFullName, Color playerColor, string _playerName)
    {
        foreach (Constellation constellation in constellations)
        {
            // remove ownership if this user had a different constellation selected
            if (constellation.playerName == _playerName && constellation.constellationNameFull != cFullName)
            {
                constellation.playerName = "";
            }
            // add ownership
            if (constellation.constellationNameFull == cFullName)
            {
                constellation.playerName = _playerName;
            }
            if (constellation.playerName == "" || constellation.playerName == _playerName)
            {
                constellation.HighlightConstellationLines(constellation.constellationNameFull == cFullName, playerColor);
                constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
            }
        }
    }

    public void HighlightAllConstellations(bool highlight)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.HighlightConstellationStars(highlight);
            constellation.ShowConstellationLines(highlight, lineWidth);
        }
    }

    public void ShowAllConstellations(bool show)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.ShowConstellationLines(show, lineWidth);
        }
    }

    public void SelectConstellationByName(string constellationName)
    {
        CCDebug.Log("Selected constellation " + constellationName, LogLevel.Info, LogMessageCategory.Interaction);

        if (constellationName.ToLower() == SimulationConstants.CONSTELLATIONS_ALL)
        {
            HighlightAllConstellations(true);
            manager.CurrentlySelectedStar = null;
            manager.CurrentlySelectedConstellation = SimulationConstants.CONSTELLATIONS_ALL;
            SimulationEvents.Instance.StarSelected.Invoke(null, manager.LocalUsername, manager.LocalPlayerColor);
            SimulationEvents.Instance.ConstellationSelected.Invoke(SimulationConstants.CONSTELLATIONS_ALL);
        }
        else if (constellationName.ToLower() == SimulationConstants.CONSTELLATIONS_NONE)
        {
            HighlightAllConstellations(false);
            manager.CurrentlySelectedStar = null;
            manager.CurrentlySelectedConstellation = SimulationConstants.CONSTELLATIONS_NONE;
            SimulationEvents.Instance.StarSelected.Invoke(null, manager.LocalUsername, manager.LocalPlayerColor);
            SimulationEvents.Instance.ConstellationSelected.Invoke(SimulationConstants.CONSTELLATIONS_NONE);
        }
        else
        {
            List<Star> allStarsInConstellation = DataManager.Instance.AllStarsInConstellationByFullName(constellationName);
            CCDebug.Log("Count of stars: " + allStarsInConstellation.Count, LogLevel.Info, LogMessageCategory.Interaction);
            if (allStarsInConstellation.Count > 0)
            {
                Star brightestStar = allStarsInConstellation.OrderBy(s => s.Mag).FirstOrDefault();
                CCDebug.Log(brightestStar.ProperName, LogLevel.Info, LogMessageCategory.Interaction);
                SimulationEvents.Instance.StarSelected.Invoke(brightestStar, manager.LocalUsername, manager.LocalPlayerColor);
                DataController dc = manager.DataControllerComponent;
                StarComponent sc = dc.GetStarById(brightestStar.uniqueId);
                manager.CurrentlySelectedStar = sc;
                manager.CurrentlySelectedConstellation = brightestStar.ConstellationFullName;
                SimulationEvents.Instance.StarSelected.Invoke(brightestStar, manager.LocalUsername, manager.LocalPlayerColor);
                SimulationEvents.Instance.ConstellationSelected.Invoke(brightestStar.ConstellationFullName);
                // broadcast selection
                InteractionController interactionController = FindObjectOfType<InteractionController>();
                interactionController.ShowCelestialObjectInteraction(brightestStar.ProperName,
                    brightestStar.Constellation, brightestStar.uniqueId, true);
            }
            HighlightSingleConstellation(constellationName, manager.LocalPlayerColor, manager.LocalUsername);
        }
    }

}
