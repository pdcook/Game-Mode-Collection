﻿using HarmonyLib;
using UnityEngine;
using UnboundLib;
using UnboundLib.GameModes;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Collections.Generic;
using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Roles;

namespace GameModeCollection.Patches
{
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_Assassin
    {
        // prefix for the assassin role in TRT
        private static void Prefix(HealthHandler __instance, ref Vector2 damage, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer?.GetComponent<Assassin>() is null) { return; }

            if (damagingPlayer.GetComponent<Assassin>().Target?.playerID == __instance?.GetComponent<Player>()?.playerID)
            {
                damage = Assassin.TargetMultiplier * damage;
            }
            else
            {
                damage = Assassin.NonTargetMultiplier * damage;
            }
        }
    }
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_NoDamage
    {
        // patch for TRT roles that cannot deal damage
        private static void Prefix(ref Vector2 damage, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer?.GetComponent<ITRT_Role>() is null) { return; }

            if (!damagingPlayer.GetComponent<ITRT_Role>().CanDealDamage)
            {
                damage = Vector2.zero;
            }
        }
    }
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandlerPatchDoDamage_Friendly_Self_Enemy_Damage
    {
        // prefix to disable damage for friendly fire and self damage
        private static bool Prefix(HealthHandler __instance, Vector2 damage, Vector2 position, Color blinkColor, GameObject damagingWeapon = null, Player damagingPlayer = null, bool healthRemoval = false, bool lethal = true, bool ignoreBlock = false)
        {
            Player ownPlayer = __instance.GetComponent<Player>();
            if (damagingPlayer != null && ownPlayer != null)
            {
                // self-damage
                if (damagingPlayer.playerID == ownPlayer.playerID && !GameModeCollection.SelfDamageAllowed)
                {
                    return false;
                }
                // friendly fire
                else if (damagingPlayer.playerID != ownPlayer.playerID && damagingPlayer.teamID == ownPlayer.teamID && !GameModeCollection.TeamDamageAllowed)
                {
                    return false;
                }
                // enemy fire
                else if (damagingPlayer.playerID != ownPlayer.playerID && damagingPlayer.teamID != ownPlayer.teamID && !GameModeCollection.EnemyDamageAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
    [HarmonyPatch(typeof(HealthHandler), nameof(HealthHandler.Revive))]
    class HealthHandler_Patch_Revive
    {
        static void Prefix(HealthHandler __instance, bool isFullRevive)
        {
            if (isFullRevive && GameModeCollection.CreatePlayerCorpses)
            {
                __instance.ReviveCorpse();
            }
        }
        static void Postfix(HealthHandler __instance, bool isFullRevive)
        {
            if (isFullRevive)
            {
                ((CharacterData)__instance.GetFieldValue("data")).lastDamagedPlayer = null;
                ((CharacterData)__instance.GetFieldValue("data")).lastSourceOfDamage = null;
                if (__instance.GetComponent<TRT_Corpse>() != null)
                {
                    UnityEngine.GameObject.Destroy(__instance.GetComponent<TRT_Corpse>());
                }
            }
        }
    }
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die")]
    class HealthHandler_Patch_RPCA_Die
    {

        static void MakeCorpse(HealthHandler healthHandler)
        {
            if (GameModeCollection.CreatePlayerCorpses)
            {
                healthHandler.MakeCorpse();
            }
            else
            {
                healthHandler.gameObject.SetActive(false);
            }
        }

        // patch to create player corpses instead of hiding dead players
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var m_setActive = ExtensionMethods.GetMethodInfo(typeof(GameObject), nameof(GameObject.SetActive));
            var m_makeCorpse = ExtensionMethods.GetMethodInfo(typeof(HealthHandler_Patch_RPCA_Die), nameof(MakeCorpse));

            List<CodeInstruction> codes = instructions.ToList();

            int index = -1;

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].Calls(m_setActive))
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                GameModeCollection.LogError("[HeathHandler::RPCA_Die Patch] INSTRUCTION NOT FOUND");
            }

            codes[index] = new CodeInstruction(OpCodes.Nop);
            codes[index-1] = new CodeInstruction(OpCodes.Nop);
            codes[index-2] = new CodeInstruction(OpCodes.Call, m_makeCorpse);

            return codes.AsEnumerable();
        }
    }
}
