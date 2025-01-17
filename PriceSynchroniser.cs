using Steamworks;
using UnityEngine;

namespace ContentWarningShop
{
    internal class PriceSynchroniser
    {
        private static Callback<LobbyCreated_t> cb_onLobbyCreated;
        private static Callback<LobbyEnter_t> cb_onLobbyEntered;
        private static bool IsHost => SteamMatchmaking.GetLobbyOwner(LobbyID) == SteamUser.GetSteamID();
        private static CSteamID LobbyID {  get; set; }

        internal static void RegisterCallbacks()
        {
            cb_onLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            cb_onLobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        private static void OnLobbyCreated(LobbyCreated_t e)
        {
            if (e.m_eResult != EResult.k_EResultOK)
            {
                return;
            }
            LobbyID = new(e.m_ulSteamIDLobby);
            foreach (var item in Shop._items)
            {
                var key = $"__{item.PersistentID}_price";
                SteamMatchmaking.SetLobbyData(LobbyID, key, $"{item.price}");
                Debug.Log($"Item price registered: {item.name} ({key}) = {item.price}");
            }
        }

        private static void OnLobbyEntered(LobbyEnter_t e)
        {
            if (IsHost == true)
            {
                return;
            }
            LobbyID = new(e.m_ulSteamIDLobby);
            foreach (var item in Shop._items)
            {
                var key = $"__{item.PersistentID}_price";
                var strVal = SteamMatchmaking.GetLobbyData(LobbyID, key);
                item.price = int.Parse(strVal);
                Debug.Log($"Item price synchronised: {item.name} ({key}) = {item.price}");
            }
        }
    }
}
