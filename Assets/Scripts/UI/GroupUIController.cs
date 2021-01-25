using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupUIController : MonoBehaviour
{
   [SerializeField] private GroupMemberButton[] playerButtons;
   private SimulationManager manager { get { return SimulationManager.Instance; } }
   private SimulationEvents events { get { return SimulationEvents.Instance; } }
   private void OnEnable()
   {
      events.PlayerJoined.AddListener(playerListChanged);
      events.PlayerLeft.AddListener(playerListChanged);
      events.NetworkConnection.AddListener(connectionStatusUpdated);
      playerListChanged(manager.LocalUsername);
   }

   void resetPlayerButtons()
   {
      Debug.Log("clearing player buttons");
      for (int i = 0; i < playerButtons.Length; i++)
      {
         GroupMemberButton b = playerButtons[i];
         b.Reset();
      }
   }
   private void connectionStatusUpdated(bool isConnected)
   {
      CCDebug.Log("Connection Status: " + isConnected, LogLevel.Info, LogMessageCategory.Networking);
      if (!isConnected)
      {
         resetPlayerButtons();
      }
   }
   private void playerListChanged(string playerName)
   {
      // Clear the list
      resetPlayerButtons();

      // update network players
      if (manager.AllRemotePlayers.Length > 0)
      {
         int maxButtons = Mathf.Clamp(manager.AllRemotePlayers.Length, 0, playerButtons.Length);
         for (int i = 0; i < maxButtons; i++)
         {
            Player p = manager.AllRemotePlayers[i];
            GroupMemberButton b = playerButtons[i];
            if (p != null)
            {
               b.name = p.Name;
               b.Setup(p);
            }
         }
      }
   }
   private void OnDisable()
   {
      events.PlayerJoined.RemoveListener(playerListChanged);
      events.PlayerLeft.RemoveListener(playerListChanged);
      events.NetworkConnection.RemoveListener(connectionStatusUpdated);
   }
}
