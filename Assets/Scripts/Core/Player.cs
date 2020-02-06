using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string Name;
    public UserRecord PlayerUserRecord;
    public Pushpin Pin;
    public Vector3 AvatarPosition;
    public Quaternion AvatarRotation;
    public Vector3 CameraDirection;
    public CelestialSphereItem SelectedCelestialSphereItem;
    public List<AnnotationLine> AnnotationLines;
    public string CurrentScene;

    // Remote players don't need a full record, just a name will do
    public Player(string playerName)
    {
        Name = playerName;
        Pin = new Pushpin();
    }
    public Player(UserRecord userRecord)
    {
        PlayerUserRecord = userRecord;
        Name = userRecord.Username;
        Pin = new Pushpin();
    }
    public Player(UserRecord userRecord, Pushpin pin)
    {
        PlayerUserRecord = userRecord;
        Pin = pin;
    }

    public void UpdatePlayerPin(Pushpin pushpin)
    {
        this.Pin = pushpin;
    }

    public void UpdatePlayerLocation(LatLng latLng)
    {
        this.Pin.Location = latLng;
    }

    public void UpdatePlayerLookDirection(Vector3 cameraDirection)
    {
        this.CameraDirection = cameraDirection;
    }
}
