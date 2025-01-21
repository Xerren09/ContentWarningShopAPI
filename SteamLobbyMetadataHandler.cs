using Steamworks;
using UnityEngine;

namespace ContentWarningShop
{
    internal static class SteamLobbyMetadataHandler
    {
        private static Callback<LobbyChatUpdate_t> cb_onLobbyStatusUpdate;
        private static Callback<LobbyCreated_t> cb_onLobbyCreated;
        private static Callback<LobbyEnter_t> cb_onLobbyEntered;
        private static Callback<LobbyDataUpdate_t> cb_onLobbyDataUpdate;
        //
        internal static event Action? OnLobbyJoined;
        internal static event Action? OnLobbyLeft;
        internal static event Action? OnLobbyDataUpdate;

        internal static CSteamID CurrentLobby;
        internal static bool InLobby => CurrentLobby != default;
        internal static bool IsHost => InLobby && SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();

        static SteamLobbyMetadataHandler()
        {
            cb_onLobbyStatusUpdate = Callback<LobbyChatUpdate_t>.Create(Steam_LobbyLeft);
            cb_onLobbyCreated = Callback<LobbyCreated_t>.Create(Steam_LobbyCreated);
            cb_onLobbyEntered = Callback<LobbyEnter_t>.Create(Steam_LobbyEntered);
            cb_onLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(Steam_LobbyDataUpdated);
        }

        private static void Steam_LobbyCreated(LobbyCreated_t e)
        {
            if (e.m_eResult == EResult.k_EResultOK)
            {
                CurrentLobby = new CSteamID(e.m_ulSteamIDLobby);
                OnLobbyJoined?.Invoke();
            }
        }

        private static void Steam_LobbyEntered(LobbyEnter_t e)
        {
            // If we created the lobby, don't call the event again. Otherwise _currentLobby will be default by now
            if (CurrentLobby == default)
            {
                CurrentLobby = new CSteamID(e.m_ulSteamIDLobby);
                OnLobbyJoined?.Invoke();
            }
        }

        private static void Steam_LobbyLeft(LobbyChatUpdate_t e)
        {
            var user = new CSteamID(e.m_ulSteamIDUserChanged);
            if (user != SteamUser.GetSteamID())
            {
                return;
            }
            if ((EChatMemberStateChange)e.m_rgfChatMemberStateChange != EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                // Lobby left
                CurrentLobby = default;
                OnLobbyLeft?.Invoke();
            }
        }

        private static void Steam_LobbyDataUpdated(LobbyDataUpdate_t e)
        {
            if (InLobby == false || e.m_ulSteamIDLobby != e.m_ulSteamIDMember)
            {
                return;
            }
            OnLobbyDataUpdate?.Invoke();
        }
    }
}
