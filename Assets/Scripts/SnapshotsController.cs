using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
///  Snapshots saved and loaded from user player prefs
/// </summary>
public class SnapshotsController : MonoBehaviour
{
    private SimulationManager manager { get { return SimulationManager.GetInstance();}}

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

    private bool hasLoadedSnapshots = false;
    public void Init()
    {
        if (!hasLoadedSnapshots)
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
                    Pushpin snapPin = new Pushpin(dt, locationCoords, location);
                    manager.LocalUserSnapshots.Add(snapPin);
                }

                count++;
            } while (snapshotFound);
            // snapshots should now all be loaded and saved in the manager
            hasLoadedSnapshots = true;
        }
    }

    public void SaveSnapshot(Pushpin pin, int pinIndex)
    {
        PlayerPrefs.SetString(locationKey + pinIndex, pin.LocationName);
        PlayerPrefs.SetString(datetimeKey + pinIndex, pin.SelectedDateTime.ToString());
        PlayerPrefs.SetString(coordsKey + pinIndex, pin.Location.ToString());
        SimulationEvents.GetInstance().SnapshotCreated.Invoke(pin);
    }

    public void DeleteSnapshot(Pushpin snapshot)
    {
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
        for (int i = 0; i < manager.LocalUserSnapshots.Count; i++)
        {
            PlayerPrefs.SetString(locationKey + (i).ToString(), manager.LocalUserSnapshots[i].LocationName);
            PlayerPrefs.SetString(datetimeKey + (i).ToString(), manager.LocalUserSnapshots[i].SelectedDateTime.ToString());
            PlayerPrefs.SetString(coordsKey + (i).ToString(), manager.LocalUserSnapshots[i].Location.ToString());
        }
        SimulationEvents.GetInstance().SnapshotDeleted.Invoke(snapshot);
    }
}
