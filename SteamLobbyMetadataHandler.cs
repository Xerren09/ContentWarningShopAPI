using Steamworks;

namespace ContentWarningShop
{
    internal static class SteamLobbyMetadataHandler
    {
        private static bool _initialised = false;

        private static Callback<LobbyCreated_t> cb_onLobbyCreated;
        private static Callback<LobbyEnter_t> cb_onLobbyEntered;
        private static Callback<LobbyDataUpdate_t> cb_onLobbyDataUpdate;
        //
        internal static event Action? OnLobbyJoined;
        internal static event Action? OnLobbyCreated;
        internal static event Action? OnLobbyDataUpdate;
        /// <summary>
        /// The ID of the current lobby, if we are in one.
        /// </summary>
        /// <remarks>
        /// Cleared automatically in <see cref="ShopAPI.Patches.SteamLobbyHandlerPatch.LeaveLobby"/> patch whenever the player clicks Leave.
        /// </remarks>
        internal static CSteamID CurrentLobby = CSteamID.Nil;

        internal static bool InLobby => CurrentLobby != CSteamID.Nil;
        internal static bool IsHost => SteamMatchmaking.GetLobbyOwner(CurrentLobby) == SteamUser.GetSteamID();

        internal static void RegisterSteamworksCallbacks()
        {
            if (_initialised) return;

            cb_onLobbyCreated = Callback<LobbyCreated_t>.Create(Steam_LobbyCreated);
            cb_onLobbyEntered = Callback<LobbyEnter_t>.Create(Steam_LobbyEntered);
            cb_onLobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(Steam_LobbyDataUpdated);
        }

        private static void Steam_LobbyCreated(LobbyCreated_t e)
        {
            if (e.m_eResult == EResult.k_EResultOK)
            {
                CurrentLobby = new CSteamID(e.m_ulSteamIDLobby);
                OnLobbyCreated?.Invoke();
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
