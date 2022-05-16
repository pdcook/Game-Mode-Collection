using HarmonyLib;
using GameModeCollection;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Block))]
    class BlockPatches
    {
        // patches for different default block cooldown multipliers
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void Start(Block __instance)
        {
            __instance.cdMultiplier = GameModeCollection.DefaultBlockCooldownMultiplier;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch("ResetStats")]
        static void ResetStats(Block __instance)
        {
            __instance.cdMultiplier = GameModeCollection.DefaultBlockCooldownMultiplier;
        }
    }
}
