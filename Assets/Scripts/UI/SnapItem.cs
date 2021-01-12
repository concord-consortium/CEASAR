using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SnapItem : MonoBehaviour
{
    public TMP_Text snapItemText;
    [SerializeField]
    private Pushpin snapshot;

    public GameObject pinButton;
    public GameObject deleteButton;

    private MainUIController mainUIController;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
        MenuOption pb = pinButton.GetComponent<MenuOption>();
        pb.OnClick.AddListener( () => RestoreSnapItem());
        MenuOption db = deleteButton.GetComponent<MenuOption>();
        db.OnClick.AddListener(() => DeleteSnapItem());
    }

    public void SetSnapPin(Pushpin pin)
    {
        snapshot = new Pushpin(pin.SelectedDateTime, pin.Location, pin.LocationName);
    }
    public void DeleteSnapItem()
    {
        mainUIController.DeleteSnapshot(this.snapshot);
        Destroy(this.gameObject);
    }

    public void RestoreSnapItem()
    {
        CCDebug.Log($"Restoring my snapshot {this.snapshot}", LogLevel.Info, LogMessageCategory.Event);
        Pushpin p = new Pushpin(this.snapshot.SelectedDateTime, this.snapshot.Location, this.snapshot.LocationName);
        mainUIController.RestoreSnapshot(p);
    }
}
