using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstellationManager : MonoBehaviour
{
    List<Constellation> constellations = new List<Constellation>();

    public void AddConstellation(Constellation constellation)
    {
        constellations.Add(constellation);
    }

	public Constellation GetConstellation(string cname)
	{
		int index = constellations.FindIndex(el => el.constellationNameAbbr == cname);
        if (index < 0)
        {
            return constellations[index];
        }
        else
        {
            return null;
        }
	}

	public void HighlightSingleConstellation(string cname)
	{
        foreach (Constellation constellation in constellations)
        {
            constellation.Highlight(constellation.constellationNameAbbr == cname);
            constellation.ShowConstellationLines(constellation.constellationNameAbbr == cname);
        }
	}

    public void HighlightAllConstellations(bool highlight)
	{
        foreach (Constellation constellation in constellations)
        {
            constellation.Highlight(highlight);
            constellation.ShowConstellationLines(highlight);
        }
	}

	public void ShowSingleConstellation(string cname)
	{
        foreach (Constellation constellation in constellations)
        {
            constellation.ShowConstellationLines(constellation.constellationNameAbbr == cname);
        }
	}

    public void ShowAllConstellations()
	{
        foreach (Constellation constellation in constellations)
        {
            constellation.ShowConstellationLines(true);
        }
	}

}
