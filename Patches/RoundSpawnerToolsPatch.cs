using ContentWarningShop;
using HarmonyLib;
using UnityEngine;

namespace ShopAPI.Patches
{
    [HarmonyPatch(typeof(RoundSpawnerTools))]
    internal class RoundSpawnerToolsPatch
    {
        [HarmonyPatch(nameof(RoundSpawnerTools.SpawnRound))]
        [HarmonyPrefix]
        static void SpawnRound(RoundSpawnerTools __instance)
        {
            var customSpawnables = Shop.CustomItems.Where(item => item.spawnable && item.itemType == Item.ItemType.Tool);
            Debug.Log($"Added {customSpawnables.Count()} custom items marked as spawnable to {nameof(RoundSpawnerTools)}");
            __instance.possibleSpawns.AddRange(customSpawnables);
        }
    }
}
