using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public UserRecord PlayerUserRecord;
    public Pushpin Pin;
    public Vector3 AvatarPosition;
    public Quaternion AvatarRotation;
    public Vector3 CameraDirection;
    public CelestialSphereItem SelectedCelestialSphereItem;
    public List<AnnotationLine> AnnotationLines;
    public string CurrentScene;
    
    public static bool operator ==(Player p1, Player p2) 
    {
        return p1.PlayerUserRecord.Equals(p2.PlayerUserRecord);
    }

    public static bool operator !=(Player p1, Player p2) 
    {
        return !p1.PlayerUserRecord.Equals(p2.PlayerUserRecord);
    }

    public Player(UserRecord userRecord, Pushpin pin)
    {
        PlayerUserRecord = userRecord;
        Pin = pin;
    }
}
