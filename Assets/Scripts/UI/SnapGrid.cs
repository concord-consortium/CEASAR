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

    private SimulationManager manager { get {return SimulationManager.Instance;}}
    private SimulationEvents events { get {return SimulationEvents.Instance;}}
    private void Start()
    {
        snaps = new List<GameObject>();
        foreach (Pushpin p in manager.LocalUserSnapshots)
        {
            AddSnapItem(p);
        }
        events.SnapshotCreated.AddListener(snapCreated);
        events.SnapshotLoaded.AddListener(snapLoaded);
        events.SnapshotDeleted.AddListener(snapDeleted);
    }

    void snapCreated(Pushpin p)
    {
        AddSnapItem(p);
    }

    void snapLoaded(Pushpin p) { }
    void snapDeleted(Pushpin p) { }

  
    public void AddSnapItem(Pushpin newSnap)
    {
        if (snaps == null) snaps = new List<GameObject>();
        GameObject snapItem = Instantiate(snapItemPrefab, transform);
        snapItem.GetComponent<SnapItem>().snapItemText.text = newSnap.SnapshotText;
        snapItem.GetComponent<SnapItem>().SetSnapPin(newSnap);
        snaps.Add(snapItem);
    }
}
