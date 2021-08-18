using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupUserButton : MonoBehaviour
{
    public GameObject Name;
    public GameObject GroupImage;
    public GameObject UserImage;
    public GameObject HL2Button;
    private Player _player;
    private SimulationManager manager { get { return SimulationManager.Instance; } }
    private SimulationEvents events { get { return SimulationEvents.Instance; } }

    public void Setup(Player p, string playerName, Color color, bool isCurrentUser)
    {
        Name.GetComponent<TMP_Text>().text = playerName;
        Name.GetComponent<TMP_Text>().color = color;
        if (isCurrentUser)
        {
            UserImage.SetActive(true);
            UserImage.GetComponent<Image>().color = UserRecord.GetColorForUsername(playerName);
            HL2Button.SetActive(false);
            Image im = GetComponent<Image>();
            if (im) im.enabled = false;
            Button but = GetComponent<Button>();
            if (but) but.enabled = false;
        }
        else
        {
            GroupImage.SetActive(true);
            GroupImage.GetComponent<Image>().color = UserRecord.GetColorForUsername(playerName);
        }
        _player = p;
    }

    public void PlayerClicked()
    {
        if (_player != null)
        {
            Pushpin pin = _player.Pin;
            manager.JumpToPin(pin);

            // Invoking this event will update the simulation time and location;
            events.PushPinSelected.Invoke(pin);
        }
    }
}
