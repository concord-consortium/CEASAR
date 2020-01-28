﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnapshotsController : MonoBehaviour
{
    public List<Snapshot> snapshots = new List<Snapshot>();

    public void Init()
    {
        // load snapshots from playerprefs
        bool snapshotFound = false;
        int count = 0;
        do
        {
            snapshotFound = false;
            // snapshots are stored in playerprefs as paired entries with an index suffix
            // e.g., "SnapshotLocation0", "SnapshotDateTime0"
            if (PlayerPrefs.HasKey("SnapshotLocation" + count) && PlayerPrefs.HasKey("SnapshotDateTime" + count))
            {
                snapshotFound = true;
                string location = PlayerPrefs.GetString("SnapshotLocation" + count, "");
                string dts = PlayerPrefs.GetString("SnapshotDateTime" + count, "");
                DateTime dt = DateTime.Parse(dts);
                LatLng locationCoords = new LatLng(PlayerPrefs.GetString("SnapshotLocationCoords", ""));
                snapshots.Add(new Snapshot(dt, location, locationCoords));
            }
            count++;
        } while (snapshotFound);
    }

    public void CreateSnapshot(DateTime dt, String location, LatLng locationCoords)
    {
        snapshots.Add(new Snapshot(dt, location, locationCoords));
        PlayerPrefs.SetString("SnapshotLocation" + (snapshots.Count - 1).ToString(), location);
        PlayerPrefs.SetString("SnapshotDateTime" + (snapshots.Count - 1).ToString(), dt.ToString());
        PlayerPrefs.SetString("SnapshotLocationCoords" + (snapshots.Count - 1).ToString(), locationCoords.ToString());
    }

    public void DeleteSnapshot(Snapshot snapshot)
    {
        int snapshotIndex = snapshots.FindIndex(el => el.location == snapshot.location && el.dateTime == snapshot.dateTime);
        snapshots.RemoveAt(snapshotIndex);

        // delete all from playerprefs
        bool snapshotFound = false;
        int count = 0;
        do
        {
            snapshotFound = false;
            if (PlayerPrefs.HasKey("SnapshotLocation" + count) && PlayerPrefs.HasKey("SnapshotDateTime" + count))
            {
                snapshotFound = true;
                PlayerPrefs.DeleteKey("SnapshotLocation" + count);
                PlayerPrefs.DeleteKey("SnapshotDateTime" + count);
                PlayerPrefs.DeleteKey("SnapshotLocationCoords" + count);
            }
            count++;
        } while (snapshotFound);

        // add them back in
        for (int i = 0; i < snapshots.Count; i++)
        {
            PlayerPrefs.SetString("SnapshotLocation" + (i).ToString(), snapshots[i].location);
            PlayerPrefs.SetString("SnapshotDateTime" + (i).ToString(), snapshots[i].dateTime.ToString());
            PlayerPrefs.SetString("SnapshotLocationCoords" + (i).ToString(), snapshots[i].locationCoordinates.ToString());
        }
    }
}
