using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constellation : MonoBehaviour
{
    public string constellationNameAbbr;
    public string constellationNameFull;
    // the stars in the constellation
    List<GameObject> stars = new List<GameObject>();
    // the actual game objects that contain the line renderers
    List<GameObject> constellationLines = new List<GameObject>();
    //the set of constellation connections that define the constellation lines
    List<ConstellationConnection> constellationConnections = new List<ConstellationConnection>();

    public Color highlightColor = Color.red;
    public Color neutralColor = Color.white;

    public LineRenderer constellationLinePrefab;

    public void AddStar(GameObject star)
    {
        stars.Add(star);
    }
    public void AddConstellationConnection(ConstellationConnection conn)
    {
        constellationConnections.Add(conn);
    }

    public void Highlight(bool highlight)
    {
        foreach (GameObject starObject in stars)
        {
            Utils.SetObjectColor(starObject, highlight ? highlightColor : neutralColor);
        }
    }

    public void ShowConstellationLines(bool show, float width)
    {
        if (constellationLines.Count > 0)
        {
            foreach (GameObject go in constellationLines)
            {
                go.SetActive(show);
            }
        }
        else
        {
            if (show)
            {
                foreach (ConstellationConnection conn in constellationConnections)
                {
                    int startIndex = stars.FindIndex(star => star.GetComponent<StarComponent>().starData.Hipparcos == conn.startStarHipId);
                    int endIndex = stars.FindIndex(star => star.GetComponent<StarComponent>().starData.Hipparcos == conn.endStarHipId);
                    if (startIndex >= 0 && endIndex >= 0)
                    {
                        if (constellationLinePrefab)
                        {
                            LineRenderer connectionLine = Instantiate(constellationLinePrefab, this.transform);
                            connectionLine.SetPosition(0, new Vector3(stars[startIndex].transform.position.x, stars[startIndex].transform.position.y, stars[startIndex].transform.position.z));
                            connectionLine.SetPosition(1, new Vector3(stars[endIndex].transform.position.x, stars[endIndex].transform.position.y, stars[endIndex].transform.position.z));
                            connectionLine.startWidth = width;
                            connectionLine.endWidth = width;
                            constellationLines.Add(connectionLine.gameObject);
                        }
                    }
                    else
                    {
                        // if (startIndex < 0) Debug.Log("Missing Star in Constellation Connection: " + conn.startStarHipId);
                        // if (endIndex < 0) Debug.Log("Missing Star in Constellation Connection: " + conn.endStarHipId);
                    }
                }
            }
        }
    }
}
