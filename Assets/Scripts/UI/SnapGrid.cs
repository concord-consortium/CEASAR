using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnapGrid : MonoBehaviour
{
    public GameObject snapItemPrefab;
    private List<GameObject> snaps;

    private SimulationManager manager { get {return SimulationManager.GetInstance();}}

    private void Start()
    {
        snaps = new List<GameObject>();
    }
    
    public void AddSnapItem(Pushpin newSnap)
    {
        if (snaps == null) snaps = new List<GameObject>();
        GameObject snapItem = Instantiate(snapItemPrefab, transform);
        snapItem.GetComponent<SnapItem>().snapItemText.text = newSnap.SnapshotText;
        snapItem.GetComponent<SnapItem>().SetSnapPin(newSnap);
        snaps.Add(snapItem);
    }
}
