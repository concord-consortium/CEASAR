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
   [SerializeField] private TMP_Text playerName;
   private Player _player;
   private SimulationManager manager { get { return SimulationManager.Instance; } }
   private SimulationEvents events { get { return SimulationEvents.Instance; } }
   
   public void Setup(Player p)
   {
      playerName.SetText(p.DisplayName);
      Color playerColor = UserRecord.GetColorForUsername(p.Name);
      playerName.color = playerColor;
      playerIcon.color = playerColor;
      _player = p;
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
      playerIcon.color = Color.clear;
      timeIcon.color = Color.clear;
      locationIcon.color = Color.clear;
      playerName.color = Color.clear;
      playerName.SetText("");
      this.name = "playerButton";
      _player = null;
   }

   private void showTimeIcon(bool show)
   {
      if (show) timeIcon.color = Color.white;
      else timeIcon.color = Color.clear;
   }
   private void showLocationIcon(bool show)
   {
      if (show) locationIcon.color = Color.white;
      else locationIcon.color = Color.clear;
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
