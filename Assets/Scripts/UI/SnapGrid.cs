using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapGrid : MonoBehaviour
{
    public GameObject snapItemPrefab;

    public void AddSnapItem(Pushpin newSnap)
    {
        string snapText = newSnap.LocationName + ":\n" + newSnap.SelectedDateTime.ToShortDateString() + " " + newSnap.SelectedDateTime.ToShortTimeString();
        GameObject snapItem = (GameObject)Instantiate(snapItemPrefab, transform);
        snapItem.GetComponent<SnapItem>().snapItemText.text = snapText;
        snapItem.GetComponent<SnapItem>().snapshot = newSnap;
    }
}
