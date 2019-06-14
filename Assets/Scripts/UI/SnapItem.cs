using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnapItem : MonoBehaviour
{
    public TextMeshProUGUI snapItemText;
    public Snapshot snapshot;

    private MainUIController mainUIController;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
    }

    public void DeleteSnapItem()
    {
        mainUIController.DeleteSnapshot(snapshot);
        Destroy(gameObject);
    }

    public void RestoreSnapItem()
    {
        mainUIController.RestoreSnapshot(snapshot);
    }
}
