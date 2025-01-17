using ContentWarningShop;
using ContentWarningShop.Localisation;
using HarmonyLib;
using System.Reflection;
using Zorro.Core;

namespace ShopAPI.Patches
{
    [HarmonyPatch(typeof(Item))]
    internal class LocalisationPatches
    {
        private static MethodInfo __parseTip;
        private static string ParseTip(Item __instance, string tipStr, out IMKbPromptProvider provider, out List<ControllerGlyphs.GlyphType> glyphTypes)
        {
            // Cache method reference. For context, this is an inline local method within Item.GetTootipData, so it can't be referenced by name.
            // I have the IL name, but I have no guarantee that it won't change, while the method name is pretty self explanatory so I doubt they'd rename it.
            // This way I get to not update the mod every time there is a game update.
            if (__parseTip == null)
            {
                var methods = typeof(Item).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                __parseTip = Array.Find(methods, m => m.Name.Contains("ParseTip"));
            }
            var pParams = new object[] { tipStr, null, null };
            string parsedTip = (string)__parseTip.Invoke(__instance, pParams);
            parsedTip = StringUtility.EnsureSpaceAfterPhrase(parsedTip, "{key}");
            provider = (IMKbPromptProvider)pParams[1];
            glyphTypes = (List<ControllerGlyphs.GlyphType>)pParams[2];
            return parsedTip;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Item.GetTootipData))]
        private static bool GetTootipData(Item __instance, ref IEnumerable<IHaveUIData> __result)
        {
            if (Shop.IsItemRegistered(__instance) == false)
            {
                return true;
            }
            if (__instance.Tooltips.Count == 0)
            {
                return true;
            }
            // Parse glyphs into the default tooltips if they weren't localised, without overwriting them on the item (fallback if the item was not localised)
            string key = __instance.name.Trim().Replace(" ", "") + ShopLocalisation.TooltipsSuffix;
            IEnumerable<string> locTooltips = __instance.Tooltips.Select(ikt => ikt.m_text);
            var ret = new List<ItemKeyTooltip>();
            if (ShopLocalisation.TryGetLocaleString(key, out var localeStr))
            {
                locTooltips = localeStr.Split(";");
            }
            foreach (var tip in locTooltips)
            {
                string parsedTip = ParseTip(__instance, tip, out IMKbPromptProvider provider, out List<ControllerGlyphs.GlyphType> glyphType);
                ret.Add(new ItemKeyTooltip(parsedTip, provider, glyphType));
            }
            __result = ret;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Item.GetLocalizedDisplayName))]
        private static bool GetLocalizedDisplayName(Item __instance, ref string __result)
        {
            if (Shop.IsItemRegistered(__instance) == false)
            {
                return true;
            }
            var key = __instance.name.Trim().Replace(" ", "");
            if (ShopLocalisation.TryGetLocaleString(key, out var result))
            {
                if (string.IsNullOrEmpty(result) == false)
                {
                    __result = result;
                    return false;
                }
            }
            __result = __instance.displayName;
            return false;
        }
    }
}
