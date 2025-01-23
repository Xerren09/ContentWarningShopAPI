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

        private static void SyncPrices()
        {
            foreach (var item in Shop._items)
            {
                SyncPrice(item);
            }
        }

        internal static void SyncPrice(Item item)
        {
            if (SteamLobbyMetadataHandler.InLobby == false)
            {
                return;
            }
            var key = $"{ShopApiPlugin.MOD_GUID}_item_{item.PersistentID}_price";
            var changed = false;
            if (SteamLobbyMetadataHandler.IsHost)
            {
                SteamMatchmaking.SetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, key, $"{item.price}");
                changed = true;
            }
            else
            {
                var strVal = SteamMatchmaking.GetLobbyData(SteamLobbyMetadataHandler.CurrentLobby, key);
                var val = int.Parse(strVal);
                changed = item.price != val;
                item.price = val;
            }
            if (changed && ShopHandler.Instance != null)
            {
                if (ShopHandler.Instance.m_CategoryItemDic.ContainsKey(item.Category) == false)
                {
                    return;
                }
                var idx = ShopHandler.Instance.m_CategoryItemDic[item.Category].FindIndex(shopItem => shopItem.Item == item);
                var newItem = new ShopItem(item);
                ShopHandler.Instance.m_CategoryItemDic[item.Category][idx] = newItem;
                ShopHandler.Instance.m_ItemsForSaleDictionary[item.id] = newItem;
                // Does not actually call the RPC, this is the local effect
                ShopHandler.Instance.RPCA_ClearCart();
            }
            Debug.Log($"Item price synchronised: {item.name} ({key}) = {item.price}");
        }
    }
}
