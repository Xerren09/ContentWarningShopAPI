using ContentWarningShop;
using HarmonyLib;
using Steamworks;

namespace ShopAPI.Patches
{
    [HarmonyPatch(typeof(SteamLobbyHandler))]
    internal class SteamLobbyHandlerPatch
    {
        [HarmonyPatch(nameof(SteamLobbyHandler.LeaveLobby))]
        [HarmonyPostfix]
        static void LeaveLobby()
        {
            SteamLobbyMetadataHandler.CurrentLobby = CSteamID.Nil;
        }
    }
}
