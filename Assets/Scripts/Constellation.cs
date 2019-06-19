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

    public GameObject constellationConnectionPrefab;

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
            // Ensure all lines are created at start, then hidden until needed
            foreach (ConstellationConnection conn in constellationConnections)
            {
                int startIndex = stars.FindIndex(star => star.GetComponent<StarComponent>().starData.Hipparcos == conn.startStarHipId);
                int endIndex = stars.FindIndex(star => star.GetComponent<StarComponent>().starData.Hipparcos == conn.endStarHipId);
                if (startIndex >= 0 && endIndex >= 0)
                {
                    if (constellationConnectionPrefab)
                    {
                        Vector3 offset = stars[endIndex].transform.position - stars[startIndex].transform.position;
                        Vector3 scale = new Vector3(width, offset.magnitude, width);
                        Vector3 position = stars[startIndex].transform.position + (offset / 2.0f);
                        GameObject connectionLine = Instantiate(constellationConnectionPrefab, position, Quaternion.identity, this.transform);
                        connectionLine.transform.up = offset;
                        connectionLine.transform.localScale = scale;
                        constellationLines.Add(connectionLine.gameObject);
                        connectionLine.SetActive(show);
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
