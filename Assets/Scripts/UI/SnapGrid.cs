using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapGrid : MonoBehaviour
{
    public GameObject snapItemPrefab;

    public void AddSnapItem(Snapshot newSnap)
    {
        string snapText = newSnap.location + ":\n" + newSnap.dateTime.ToShortDateString() + " " + newSnap.dateTime.ToShortTimeString();
        GameObject snapItem = (GameObject)Instantiate(snapItemPrefab, transform);
        snapItem.GetComponent<SnapItem>().snapItemText.text = snapText;
        snapItem.GetComponent<SnapItem>().snapshot = newSnap;
    }
}
