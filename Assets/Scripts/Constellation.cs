﻿using System.Collections;
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
    public Color lineColor = Color.white;
    public Material lineMaterial;
    public float lineWidth = .035f;

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

    public void ShowConstellationLines(bool show)
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
                        GameObject connectionLine = new GameObject();
                        connectionLine.name = "ConnectionLine";
                        connectionLine.transform.parent = this.transform;
                        LineRenderer lineRenderer = connectionLine.AddComponent<LineRenderer>();
                        lineRenderer.startWidth = lineWidth;
                        lineRenderer.endWidth = lineWidth;
                        lineRenderer.useWorldSpace = false;
                        lineRenderer.SetPosition(0, new Vector3(stars[startIndex].transform.position.x, stars[startIndex].transform.position.y, stars[startIndex].transform.position.z));
                        lineRenderer.SetPosition(1, new Vector3(stars[endIndex].transform.position.x, stars[endIndex].transform.position.y, stars[endIndex].transform.position.z));
                        lineRenderer.material = lineMaterial;
                        lineRenderer.material.color = lineColor;
                        constellationLines.Add(connectionLine);
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