using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnapshotsController : MonoBehaviour
{
    public List<Snapshot> snapshots = new List<Snapshot>();

    void Awake()
    {
        // load snapshots from playerprefs
        bool snapshotFound = false;
        int count = 0;
        do
        {
            snapshotFound = false;
            if (PlayerPrefs.HasKey("SnapshotLocation" + count) && PlayerPrefs.HasKey("SnapshotDateTime" + count))
            {
                snapshotFound = true;
                string location = PlayerPrefs.GetString("SnapshotLocation" + count, "");
                string dts = PlayerPrefs.GetString("SnapshotDateTime" + count, "");
                DateTime dt = DateTime.Parse(dts);
                snapshots.Add(new Snapshot(dt, location));
            }
            count++;
        } while (snapshotFound);
    }

    public void AddSnapshot(DateTime dt, String location)
    {
        snapshots.Add(new Snapshot(dt, location));
        PlayerPrefs.SetString("SnapshotLocation" + (snapshots.Count - 1).ToString(), location);
        PlayerPrefs.SetString("SnapshotDateTime" + (snapshots.Count - 1).ToString(), dt.ToString());
    }

    public void GetSnapshot(int index)
    {
        Snapshot snap = snapshots[index];
    }
}
