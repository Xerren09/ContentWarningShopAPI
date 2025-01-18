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
        public const string MOD_VER = "1.0.1";

#if STEAM
        static ShopApiPlugin()
        {
            Debug.Log($"{MOD_GUID} initialised via the vanilla mod loader.");
        }
#elif MODMAN
        private Harmony harmony = new Harmony(MOD_GUID);
        void Awake()
        {
            harmony.PatchAll();
            Debug.Log($"{MOD_GUID} initialised via BepInEx mod loader.");
        }
#endif
    }
}
