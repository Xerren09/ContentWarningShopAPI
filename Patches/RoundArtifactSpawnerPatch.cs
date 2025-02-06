using ContentWarningShop;
using HarmonyLib;

namespace ShopAPI.Patches
{
    [HarmonyPatch(typeof(RoundArtifactSpawner))]
    internal class RoundArtifactSpawnerPatch
    {
        [HarmonyPatch(nameof(RoundArtifactSpawner.SpawnRound))]
        [HarmonyPrefix]
        static void SpawnRound(RoundArtifactSpawner __instance)
        {
            var customSpawnables = Shop.CustomItems.Where(item => item.spawnable);
            UnityEngine.Debug.Log($"Added {customSpawnables.Count()} custom items marked as spawnable to {nameof(RoundArtifactSpawner)}");
            __instance.possibleSpawns = (Item[])__instance.possibleSpawns.Concat(customSpawnables);
        }
    }
}
