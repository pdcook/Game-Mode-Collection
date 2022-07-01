using HarmonyLib;
using GameModeCollection;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnboundLib;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Block))]
    class BlockPatches
    {
        // patch for gamemodes that allow dropping cards
        [HarmonyPatch(typeof(Block), "ResetStats")]
        static class BlockPatchResetStats
        {
            static void ResetSinceBlockIfRevive(Block block, float val)
            {
                if (GameModeCollection.ReviveOnCardAdd)
                {
                    block.sinceBlock = val;
                }
            }
            static void ResetCounterIfRevive(Block block, float val)
            {
                if (GameModeCollection.ReviveOnCardAdd)
                {
                    block.counter = val;
                }
            }
            static void ResetBlockedThisFrameIfRevive(Block block, bool val)
            {
                if (GameModeCollection.ReviveOnCardAdd)
                {
                    block.blockedThisFrame = val;
                }
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var f_sinceBlock = ExtensionMethods.GetFieldInfo(typeof(Block), nameof(Block.sinceBlock));
                var f_counter = ExtensionMethods.GetFieldInfo(typeof(Block), nameof(Block.counter));
                var f_blockedThisFrame = ExtensionMethods.GetFieldInfo(typeof(Block), nameof(Block.blockedThisFrame));

                var m_resetSinceBlockIfRevive = ExtensionMethods.GetMethodInfo(typeof(BlockPatchResetStats), nameof(ResetSinceBlockIfRevive));
                var m_resetCounterIfRevive = ExtensionMethods.GetMethodInfo(typeof(BlockPatchResetStats), nameof(ResetCounterIfRevive));
                var m_resetBlockedThisFrameIfRevive = ExtensionMethods.GetMethodInfo(typeof(BlockPatchResetStats), nameof(ResetBlockedThisFrameIfRevive));

                foreach (var code in instructions)
                {
                    if (code.StoresField(f_sinceBlock))
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_resetSinceBlockIfRevive);
                    }
                    else if (code.StoresField(f_counter))
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_resetCounterIfRevive);
                    }
                    else if (code.StoresField(f_blockedThisFrame))
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_resetBlockedThisFrameIfRevive);
                    }
                    else
                    {
                        yield return code;
                    }
                }
            }
        }


        // patches for different default block cooldown multipliers
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Block), "Start")]
        static void Start(Block __instance)
        {
            __instance.cdMultiplier = GameModeCollection.DefaultBlockCooldownMultiplier;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Block), "ResetStats")]
        static void ResetStats(Block __instance)
        {
            __instance.cdMultiplier = GameModeCollection.DefaultBlockCooldownMultiplier;
        }
    }
}
