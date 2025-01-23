using Steamworks;
using UnityEngine;

namespace ContentWarningShop
{
    internal static class SteamLobbyMetadataHandler
    {
        private static Callback<LobbyCreated_t> cb_onLobbyCreated;
        private static Callback<LobbyEnter_t> cb_onLobbyEntered;
        private static Callback<LobbyDataUpdate_t> cb_onLobbyDataUpdate;
        //
        internal static event Action? OnLobbyJoined;
        internal static event Action? OnLobbyDataUpdate;

        internal static CSteamID CurrentLobby = CSteamID.Nil;
        // GetLobbyOwner returns nil if the current lobby is invalid / we are not in it
        internal static bool InLobby => SteamMatchmaking.GetLobbyOwner(CurrentLobby) != CSteamID.Nil;
        internal static bool IsHost => SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();

        static SteamLobbyMetadataHandler()
        {
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
            if (InLobby == false)
            {
                CurrentLobby = new CSteamID(e.m_ulSteamIDLobby);
                OnLobbyJoined?.Invoke();
            }
        }

        private static void Steam_LobbyDataUpdated(LobbyDataUpdate_t e)
        {
            if (e.m_ulSteamIDLobby != e.m_ulSteamIDMember)
            {
                return;
            }
            OnLobbyDataUpdate?.Invoke();
        }
    }
}
