using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationsController : MonoBehaviour
{

    private SimulationManager manager;
    List<Constellation> constellations = new List<Constellation>();
    float lineWidth = .035f;
    bool sceneShouldShowConstellations = false;
    
    
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

    public void HighlightSingleConstellation(string cFullName)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.Highlight(constellation.constellationNameFull == cFullName);
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }
    public void HighlightSingleConstellation(string cFullName, Color playerColor)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.Highlight(constellation.constellationNameFull == cFullName, playerColor);
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }

    public void HighlightAllConstellations(bool highlight)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.Highlight(highlight);
            constellation.ShowConstellationLines(highlight, lineWidth);
        }
    }

    public void ShowSingleConstellation(string cFullName)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.ShowConstellationLines(constellation.constellationNameFull == cFullName, lineWidth);
        }
    }

    public void ShowAllConstellations(bool show)
    {
        foreach (Constellation constellation in constellations)
        {
            constellation.ShowConstellationLines(show, lineWidth);
        }
    }

}
