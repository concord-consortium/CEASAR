using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SnapshotsController : MonoBehaviour
{
    public List<Pushpin> snapshots = new List<Pushpin>();
    
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
                snapshots.Add(new Pushpin(dt, locationCoords, location));
            }
            count++;
        } while (snapshotFound);
    }

    public void CreateSnapshot(DateTime dt, String location, LatLng locationCoords)
    {
        snapshots.Add(new Pushpin(dt, locationCoords, location));
        PlayerPrefs.SetString(locationKey + (snapshots.Count - 1).ToString(), location);
        PlayerPrefs.SetString(datetimeKey + (snapshots.Count - 1).ToString(), dt.ToString());
        PlayerPrefs.SetString(coordsKey + (snapshots.Count - 1).ToString(), locationCoords.ToString());
    }

    public void DeleteSnapshot(Pushpin snapshot)
    {
        int snapshotIndex = snapshots.FindIndex(el => el.Location == snapshot.Location && el.SelectedDateTime == snapshot.SelectedDateTime);
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
            PlayerPrefs.SetString(locationKey + (i).ToString(), snapshots[i].LocationName);
            PlayerPrefs.SetString(datetimeKey + (i).ToString(), snapshots[i].SelectedDateTime.ToString());
            PlayerPrefs.SetString(coordsKey + (i).ToString(), snapshots[i].Location.ToString());
        }
    }
}
