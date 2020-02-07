using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SnapItem : MonoBehaviour
{
    public TextMeshProUGUI snapItemText;
    [SerializeField]
    private Pushpin snapshot;

    public GameObject pinButton;
    public GameObject deleteButton;

    private MainUIController mainUIController;

    void Start()
    {
        mainUIController = FindObjectOfType<MainUIController>();
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
        mainUIController.DeleteSnapshot(this.snapshot);
        Destroy(this.gameObject);
    }

    public void RestoreSnapItem()
    {
        Debug.Log($"Restoring my snapshot {this.snapshot}");
        Pushpin p = new Pushpin(this.snapshot.SelectedDateTime, this.snapshot.Location, this.snapshot.LocationName);
        mainUIController.RestoreSnapshot(p);
    }
}
