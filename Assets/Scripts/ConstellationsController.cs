using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationsController : MonoBehaviour
{

    private SimulationManager manager;
    public List<Constellation> AllConstellations = new List<Constellation>();
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
        AllConstellations.Add(constellation);
    }

    public Constellation GetConstellation(string cFullName)
    {
        int index = AllConstellations.FindIndex(el => el.constellationNameAbbr == cFullName);
        if (index < 0)
        {
            return AllConstellations[index];
        }
        else
        {
            return null;
        }
    }

    public void HighlightSingleConstellation(string cFullName)
    {
        foreach (Constellation constellation in AllConstellations)
        {
            constellation.Highlight(constellation.constellationNameFull == cFullName);
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }
    public void HighlightSingleConstellation(string cFullName, Color playerColor)
    {
        foreach (Constellation constellation in AllConstellations)
        {
            constellation.Highlight(constellation.constellationNameFull == cFullName, playerColor);
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }

    public void HighlightAllConstellations(bool highlight)
    {
        foreach (Constellation constellation in AllConstellations)
        {
            constellation.Highlight(highlight);
            constellation.ShowConstellationLines(highlight, lineWidth);
        }
    }

    public void ShowSingleConstellation(string cFullName)
    {
        foreach (Constellation constellation in AllConstellations)
        {
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }

    public void ShowAllConstellations(bool show)
    {
        foreach (Constellation constellation in AllConstellations)
        {
            constellation.ShowConstellationLines(show, lineWidth);
        }
    }

}
