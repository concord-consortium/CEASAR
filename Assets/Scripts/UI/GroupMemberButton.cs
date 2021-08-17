using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupMemberButton : MonoBehaviour
{
   [SerializeField] private Image locationIcon;
   [SerializeField] private Image timeIcon;
   [SerializeField] private Image playerIcon;

   [SerializeField] private SpriteRenderer locationSpriteIcon;
   [SerializeField] private SpriteRenderer timeSpriteIcon;
   [SerializeField] private SpriteRenderer playerSpriteIcon;

   [SerializeField] private TMP_Text playerName;
   private Player _player;
   private SimulationManager manager { get { return SimulationManager.Instance; } }
   private SimulationEvents events { get { return SimulationEvents.Instance; } }

   public void Setup(Player p)
   {
        if (playerName)
        {
            playerName.SetText(p.DisplayName);
            Color playerColor = UserRecord.GetColorForUsername(p.Name);
            playerName.color = playerColor;
            setIconColor(playerColor, playerIcon, playerSpriteIcon);
            _player = p;
        }
   }

   void Update()
   {
      if (_player != null)
      {
         showLocationIcon(_player.Pin.Location != manager.LocalPlayerPin.Location);
         showTimeIcon(manager.PinTimeHasDiverged(_player.Pin.SelectedDateTime));
      }
   }
   public void Reset()
   {
        setIconColor(Color.clear, playerIcon, playerSpriteIcon);
        setIconColor(Color.clear, timeIcon, timeSpriteIcon);
        setIconColor(Color.clear, locationIcon, locationSpriteIcon);

        if (playerName) playerName.color = Color.clear;
        if (playerName) playerName.SetText("");
        this.name = "playerButton";
        _player = null;
   }

   private void showTimeIcon(bool show)
   {
        if (show)
        {
            setIconColor(Color.white, timeIcon, timeSpriteIcon);
        }

        else
        {
            setIconColor(Color.clear, timeIcon, timeSpriteIcon);
        }
   }
   private void showLocationIcon(bool show)
   {
        if (show)
        {
            setIconColor(Color.white, locationIcon, locationSpriteIcon);
        }
        else
        {
            setIconColor(Color.clear, locationIcon, locationSpriteIcon);
        }
   }

    private void setIconColor(Color c, Image imageIcon, SpriteRenderer spriteIcon)
    {
        // Regular UI uses Image components for icons. MRTK used SpriteRenderer in the sample, so we handle both here
        if (imageIcon) imageIcon.color = c;
        if (spriteIcon) spriteIcon.color = c;
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
