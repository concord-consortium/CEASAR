using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnapshotsController : MonoBehaviour
{
    public List<Snapshot> snapshots = new List<Snapshot>();
    
    string locationKey
    {
        get { return SimulationConstants.SNAPSHOT_LOCATION_PREF_KEY; }
    }
    string datetimeKey
    {
        get { return SimulationConstants.SNAPSHOT_DATETIME_PREF_KEY; }
    }
    string coordsKey
    {
        get { return SimulationConstants.SNAPSHOT_LOCATION_COORDS_PREF_KEY; }
    }
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
            if (PlayerPrefs.HasKey(locationKey + count) && PlayerPrefs.HasKey(datetimeKey + count))
            {
                snapshotFound = true;
                string location = PlayerPrefs.GetString(locationKey + count, "");
                string dts = PlayerPrefs.GetString(datetimeKey + count, "");
                DateTime dt = DateTime.Parse(dts);
                LatLng locationCoords = new LatLng(PlayerPrefs.GetString(coordsKey + count, ""));
                snapshots.Add(new Snapshot(dt, location, locationCoords));
            }
            count++;
        } while (snapshotFound);
    }

    public void CreateSnapshot(DateTime dt, String location, LatLng locationCoords)
    {
        snapshots.Add(new Snapshot(dt, location, locationCoords));
        PlayerPrefs.SetString(locationKey + (snapshots.Count - 1).ToString(), location);
        PlayerPrefs.SetString(datetimeKey + (snapshots.Count - 1).ToString(), dt.ToString());
        PlayerPrefs.SetString(coordsKey + (snapshots.Count - 1).ToString(), locationCoords.ToString());
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
            if (PlayerPrefs.HasKey(locationKey + count) && PlayerPrefs.HasKey(datetimeKey + count))
            {
                snapshotFound = true;
                PlayerPrefs.DeleteKey(locationKey + count);
                PlayerPrefs.DeleteKey(datetimeKey + count);
                PlayerPrefs.DeleteKey(coordsKey + count);
            }
            count++;
        } while (snapshotFound);

        // add them back in
        for (int i = 0; i < snapshots.Count; i++)
        {
            PlayerPrefs.SetString(locationKey + (i).ToString(), snapshots[i].location);
            PlayerPrefs.SetString(datetimeKey + (i).ToString(), snapshots[i].dateTime.ToString());
            PlayerPrefs.SetString(coordsKey + (i).ToString(), snapshots[i].locationCoordinates.ToString());
        }
    }
}
