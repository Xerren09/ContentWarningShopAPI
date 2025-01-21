using Steamworks;
using UnityEngine;

namespace ContentWarningShop
{
    internal class PriceSynchroniser
    {
        internal static void RegisterCallbacks()
        {
            SteamLobbyMetadataHandler.OnLobbyJoined += SyncPrices;
            SteamLobbyMetadataHandler.OnLobbyDataUpdate += OnLobbyDataUpdate;
        }

        private static void OnLobbyDataUpdate()
        {
            if (SteamLobbyMetadataHandler.IsHost)
            {
                return;
            }
            SyncPrices();
        }

        internal static void SyncPrices()
        {
            if (SteamLobbyMetadataHandler.InLobby == false)
            {
                return;
            }
            foreach (var item in Shop._items)
            {
                SyncPrice(item);
            }
        }

        internal static void SyncPrice(Item item)
        {
            var key = $"__{item.PersistentID}_price";
            if (SteamLobbyMetadataHandler.IsHost)
            {
                SteamMatchmaking.SetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, key, $"{item.price}");
            }
            else
            {
                var strVal = SteamMatchmaking.GetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, key);
                item.price = int.Parse(strVal);
            }
            Debug.Log($"Item price synchronised: {item.name} ({key}) = {item.price}");
        }
    }
}
