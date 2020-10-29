﻿using LobbyBrowserMod.Harmony;
using LobbyBrowserMod.UI;
using LobbyBrowserMod.Utils;
using System;
using System.Threading.Tasks;

namespace LobbyBrowserMod.Core
{
    public static class LobbyStateManager
    {
        private static string _lobbyCode = null;
        private static bool _didAnnounce = false;
        private static LobbyAnnounceInfo _lastCompleteAnnounce = null;
        private static bool? _localPlayerStateValue = null;

        public static string StatusText { get; private set; } = "Unknown status";
        public static bool HasErrored { get; private set; } = true;

        public static bool DidLeakCurrentCode
        {
            get
            {
                return _didAnnounce &&
                    _lastCompleteAnnounce != null
                    && !String.IsNullOrEmpty(_lobbyCode) 
                    && _lastCompleteAnnounce.ServerCode == _lobbyCode;
            }
        }

        public static void HandleLobbyCode(string lobbyCode)
        {
            if (_lobbyCode != lobbyCode)
            {
                Plugin.Log?.Info($"Got lobby server code: \"{lobbyCode}\"");

                _lobbyCode = lobbyCode;
                HandleUpdate();
            }
        }

        public static void HandleUpdate()
        {
#pragma warning disable CS4014
            var sessionManager = GameMp.SessionManager;

            if (sessionManager == null || !LobbyConnectionTypePatch.IsPartyMultiplayer
                || !LobbyConnectionTypePatch.IsPartyHost)
            {
                // We are not in a party lobby, or we are not the host
                // Make sure any previous host announcements by us are cancelled and bail
                StatusText = "You must be the host of a custom multiplayer game.";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if (Plugin.Config.LobbyAnnounceToggle)
            {
                // Toggle is on, ensure state is synced
                if (!_localPlayerStateValue.HasValue || _localPlayerStateValue.Value == false)
                {
                    _localPlayerStateValue = true;
                    sessionManager.SetLocalPlayerState("lobbyannounce", true); // NB: this calls another update
                }
            }
            else
            {
                // Toggle is off, ensure state is synced & do not proceed with announce
                StatusText = "Lobby announces are toggled off.";
                HasErrored = true;

                UnAnnounce();

                if (!_localPlayerStateValue.HasValue || _localPlayerStateValue.Value == true)
                {
                    _localPlayerStateValue = false;
                    sessionManager.SetLocalPlayerState("lobbyannounce", false); // NB: this calls another update
                }

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            if (String.IsNullOrEmpty(_lobbyCode) || !sessionManager.isConnectionOwner
                || sessionManager.localPlayer == null || !sessionManager.isConnected
                || sessionManager.maxPlayerCount == 1)
            {
                // We do not (yet) have the Server Code, or we're at an in-between state where things aren't ready yet
                StatusText = "Can't send announcement (invalid lobby state).";
                HasErrored = true;

                UnAnnounce();

                LobbyConfigPanel.UpdatePanelInstance();
                return;
            }

            var lobbyAnnounce = new LobbyAnnounceInfo()
            {
                ServerCode = _lobbyCode,
                GameName = $"{sessionManager.localPlayer.userName}'s game",
                OwnerId = sessionManager.localPlayer.userId,
                OwnerName = sessionManager.localPlayer.userName,
                PlayerCount = sessionManager.connectedPlayers.Count + 1, // + 1 for the local player host
                PlayerLimit = sessionManager.maxPlayerCount,
                IsModded = sessionManager.localPlayer.HasState("modded") && sessionManager.localPlayer.HasState("customsongs")
            };

            StatusText = "Announcing your game to the world...\r\n" + lobbyAnnounce.Describe();
            HasErrored = false;

            LobbyConfigPanel.UpdatePanelInstance();

            // TODO: Announce only if we actually have a useful update (hash announce info object?)

            DoAnnounce(lobbyAnnounce);
#pragma warning restore CS4014
        }

        private static async Task DoAnnounce(LobbyAnnounceInfo announce)
        {
            if (await MasterServerApi.SendAnnounce(announce))
            {
                _didAnnounce = true;
                _lastCompleteAnnounce = announce;

                StatusText = $"Game announced!\r\n{announce.Describe()}";
                HasErrored = false;
            }
            else
            {
                _didAnnounce = false;
                _lastCompleteAnnounce = null;

                StatusText = $"Could not announce to master server!";
                HasErrored = true;
            }

            LobbyConfigPanel.UpdatePanelInstance();
        }

        /// <summary>
        /// Ensures that any host announcements made by us are removed:
        ///  - If a previous announcement was made, a DELETE request is sent to the master server, removing it.
        ///  - If no previous announcement was made, or it was already deleted, this is a no-op.
        /// </summary>
        public static async Task UnAnnounce()
        {
            if (_lastCompleteAnnounce != null)
            {
                if (await MasterServerApi.SendDeleteAnnounce(_lastCompleteAnnounce))
                {
                    Plugin.Log?.Info($"Host announcement was deleted OK!");
                    _didAnnounce = false;
                }
            }
        }
    }
}
