using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;

public class SnapItem : MonoBehaviour
{
    public TMP_Text snapItemText;
    [SerializeField]
    private Pushpin snapshot;

    public GameObject pinButton;
    public GameObject deleteButton;

    private MainUIController mainUIController;
    private MenuController _menuController;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
        _menuController = FindObjectOfType<MenuController>();
        Button pb = pinButton.GetComponent<Button>();
        pb.onClick.AddListener( () => RestoreSnapItem());
        Button db = deleteButton.GetComponent<Button>();
        db.onClick.AddListener(() => DeleteSnapItem());
    }

    public void SetSnapPin(Pushpin pin)
    {
        snapshot = new Pushpin(pin.SelectedDateTime, pin.Location, pin.LocationName);
    }
    public void DeleteSnapItem()
    {
        if (mainUIController) mainUIController.DeleteSnapshot(this.snapshot);
        else if (_menuController) _menuController.DeleteSnapshot(this.snapshot);
        Destroy(this.gameObject);
    }

    public void RestoreSnapItem()
    {
        CCDebug.Log($"Restoring my snapshot {this.snapshot}", LogLevel.Info, LogMessageCategory.Event);
        Pushpin p = new Pushpin(this.snapshot.SelectedDateTime, this.snapshot.Location, this.snapshot.LocationName);
        if (mainUIController) mainUIController.RestoreSnapshot(p);
        else if (_menuController) _menuController.RestoreSnapshot(p);
    }
}
