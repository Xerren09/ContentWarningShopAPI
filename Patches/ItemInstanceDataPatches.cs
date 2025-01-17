using HarmonyLib;
using UnityEngine;

namespace ContentWarningShop.Patches
{
    [HarmonyPatch(typeof(ItemInstanceData))]
    internal class ItemInstanceDataPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
        private static bool GetEntryIdentifier(ref byte __result, Type type)
        {
            if (Shop._customEntries.Contains(type) == false)
            {
                return true;
            }
            var idx = Shop._customEntries.IndexOf(type);
            // Count backwards to avoid compatibility issues with the majority of mods that do the same. I've seen some stuff. It's free real estate out there.
            idx = byte.MaxValue - idx;
            if (idx < Shop._vanillaEntryCount)
            {
                throw new IndexOutOfRangeException($"{type.FullName}'s entry ID overlaps with known vanilla entry ID.");
            }
            __result = (byte)idx;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
        private static bool GetEntryType(ref ItemDataEntry __result, byte identifier)
        {
            if (Shop._customEntries.Count == 0)
            {
                return true;
            }
            if (identifier <= Shop._vanillaEntryCount)
            {
                return true;
            }
            var idx = byte.MaxValue - identifier;
            // Entry belongs to a different mod
            if (idx < 0 || idx > (Shop._customEntries.Count - 1))
            {
                return true;
            }
            var entry = Shop._customEntries.ElementAtOrDefault(idx);
            // Let the default implementation throw an error
            if (entry == default)
            {
                Debug.LogWarning($"Entry identifier {idx} should be valid but resolved to null in ShopAPI database.");
                return true;
            }
            __result = (ItemDataEntry)entry.GetConstructor(Array.Empty<Type>()).Invoke(null);
            return false;
        }
    }
}
