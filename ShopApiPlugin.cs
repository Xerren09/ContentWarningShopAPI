using UnityEngine;
#if MODMAN
using BepInEx;
using HarmonyLib;
#endif

namespace ContentWarningShop
{
    [ContentWarningPlugin(MOD_GUID, MOD_VER, false)]
#if MODMAN
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VER)]
#endif
    public class ShopApiPlugin
#if MODMAN
    : BaseUnityPlugin
#endif
    {
        public const string MOD_GUID = "xerren.cwshopapi";
        public const string MOD_NAME = "ShopAPI";
        public const string MOD_VER = ThisAssembly.AssemblyVersion;
        public const ulong STEAM_WORKSHOP_ITEM_ID = 3408837293;

#if STEAM
        static ShopApiPlugin()
        {
            SteamLobbyMetadataHandler.RegisterSteamworksCallbacks();
            ShopAPI.Logger.Log($"Initialised via the vanilla mod loader.");
        }
#elif MODMAN
        private Harmony harmony = new Harmony(MOD_GUID);
        void Awake()
        {
            harmony.PatchAll();
            SteamLobbyMetadataHandler.RegisterSteamworksCallbacks();
            ShopAPI.Logger.Log($"Initialised via BepInEx mod loader.");
        }
#endif
    }
}
