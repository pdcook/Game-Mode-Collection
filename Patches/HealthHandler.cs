using GameModeCollection.Extensions;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.GameModes.TRT.RoundEvents;
using HarmonyLib;
using Sonigon;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;

namespace GameModeCollection.Patches
{
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_RoundSummary
    {
        // postfix to log damage in TRT
        private static void Postfix(HealthHandler __instance, CharacterData ___data, Vector2 damage, GameObject damagingWeapon = null, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            RoundSummary.LogDamage(damagingPlayer, ___data.player, UnityEngine.Mathf.Clamp(damage.magnitude, 0f, ___data.health));
        }
    }
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_Claw
    {
        // prefix for Zombie claw in TRT
        private static void Prefix(HealthHandler __instance, CharacterData ___data, Vector2 damage, GameObject damagingWeapon = null, Player damagingPlayer = null)
        {
            if (damagingPlayer?.data is null || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer.playerID == __instance.GetComponent<Player>()?.playerID) { return; }

            // if the player was killed with the claw by a zombie, infect them
            if ((damagingWeapon?.GetComponent<ClawSlash>()?.isActiveAndEnabled ?? false) && damagingPlayer.GetComponent<Zombie>() != null && ___data.health - damage.magnitude < 0f && ___data.stats.remainingRespawns <= 0)
            {
                damagingPlayer.GetComponent<Zombie>().CallZombieInfect(__instance.GetComponent<Player>());
            }
        }
    }
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_Zombie
    {
        // zombies deal 50% damage with all non-claw weapons
        private static void Prefix(HealthHandler __instance, ref Vector2 damage, GameObject damagingWeapon = null, Player damagingPlayer = null)
        {
            if (damagingPlayer?.data is null || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            // if the weapon was not the claw and the player is a zombie, do 50% damage instead
            if (!(damagingWeapon?.GetComponent<ClawSlash>()?.isActiveAndEnabled ?? false) && damagingPlayer.GetComponent<Zombie>() != null)
            {
                damage = 0.5f * damage;
            }
        }
    }
 
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_Karma
    {
        // prefix for Karma in TRT 
        private static void Prefix(HealthHandler __instance, ref Vector2 damage, Player damagingPlayer = null)
        {
            if (damagingPlayer?.data is null || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer.data.TRT_Karma() >= 1f || damagingPlayer.playerID == __instance.GetComponent<Player>()?.playerID) { return; }

            damage = damagingPlayer.data.TRT_Karma() * damage;
        }
    }
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandler_Patch_DoDamage_TRT_Assassin
    {
        // prefix for the assassin role in TRT
        private static void Prefix(HealthHandler __instance, ref Vector2 damage, Player damagingPlayer = null)
        {
            if (damagingPlayer is null || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID || damagingPlayer?.GetComponent<Assassin>() is null || damagingPlayer.playerID == __instance.GetComponent<Player>()?.playerID) { return; }

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
        static bool PlayerRoleCanDealDamageAndTakeEnvironmentDamage(CharacterData data)
        {
            return data.GetComponent<ITRT_Role>()?.CanDealDamageAndTakeEnvironmentalDamage ?? true;
        }
        static bool PlayerGunIsJesterEmulator(CharacterData data)
        {
            JesterEmulatorGun jesterEmulatorGun = data.GetComponent<JesterEmulatorGun>();
            if (jesterEmulatorGun is null) { return false; }
            return (bool)jesterEmulatorGun.GetFieldValue("modifiersActive");
        }
        // patch for TRT roles that cannot deal damage or take environmental damage
        // and the JesterEmulator card
        private static void Prefix(CharacterData ___data, ref Vector2 damage, Player damagingPlayer = null)
        {
            if (GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            if (damagingPlayer is null && !PlayerRoleCanDealDamageAndTakeEnvironmentDamage(___data))
            {
                damage = Vector2.zero;
                return;
            }

            if (damagingPlayer != null && (!PlayerRoleCanDealDamageAndTakeEnvironmentDamage(damagingPlayer.data) || PlayerGunIsJesterEmulator(damagingPlayer.data)))
            {
                damage = Vector2.zero;
                return;
            }
        }
    }
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(HealthHandler), "DoDamage")]
    class HealthHandlerPatchDoDamage_Friendly_Self_Enemy_Damage
    {
        // prefix to disable damage for friendly fire and self damage
        // as well as handle invulnerability
        private static bool Prefix(HealthHandler __instance, Vector2 damage, Vector2 position, Color blinkColor, GameObject damagingWeapon = null, Player damagingPlayer = null, bool healthRemoval = false, bool lethal = true, bool ignoreBlock = false)
        {

            if (__instance.Invulnerable()) { return false; }

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
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HealthHandler), nameof(HealthHandler.Revive))]
    class HealthHandler_Patch_Revive_TRT_RoundSummary
    {
        // postfix to log revives in TRT
        private static void Postfix(HealthHandler __instance, CharacterData ___data)
        {
            if (!___data.dead || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            RoundSummary.LogEvent(ReviveEvent.ID, ___data.player.playerID);
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
        static void Postfix(HealthHandler __instance, CharacterData ___data, bool isFullRevive)
        {
            if (GameModeCollection.HideGunOnDeath)
            {
                // if the gun is hidden on death, show it when the player is revived
                ___data?.weaponHandler?.gun?.gameObject?.SetActive(true);
            }
            
            if (isFullRevive)
            {
                // fix source of last damage
                ___data.lastDamagedPlayer = null;
                ___data.lastSourceOfDamage = null;

                // destroy corpse component
                if (__instance.GetComponent<TRT_Corpse>() != null)
                {
                    UnityEngine.GameObject.Destroy(__instance.GetComponent<TRT_Corpse>());
                }
            }
        }
    }
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die_Phoenix")]
    class HealthHandler_Patch_RPCA_Die_Phoenix_TRT_RoundSummary
    {
        // prefix to log kills in TRT
        // must be a prefix since ___data.dead will always be true in the postfix
        private static void Prefix(HealthHandler __instance, CharacterData ___data)
        {
            if (___data.dead || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            RoundSummary.LogKill(___data.lastSourceOfDamage, ___data.player);
        }
    }
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die_Phoenix")]
    class HealthHandler_Patch_RPCA_Die_Phoenix
    {

        [HarmonyPriority(Priority.First)]
        static bool Prefix(HealthHandler __instance)
        {
            return !__instance.Invulnerable() && !__instance.Intangible() && (!__instance.isRespawning || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID);
        }
        static void Postfix(HealthHandler __instance, CharacterData ___data)
        {
            if (GameModeCollection.HideGunOnDeath)
            {
                // hide gun on death
                ___data?.weaponHandler?.gun?.gameObject?.SetActive(false);
            }
        }
    }
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die")]
    class HealthHandler_Patch_RPCA_Die_TRT_RoundSummary
    {
        // postfix to log kills in TRT
        // must be a prefix since ___data.dead will always be true in the postfix
        private static void Prefix(HealthHandler __instance, CharacterData ___data)
        {
            if (___data.dead || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID) { return; }

            RoundSummary.LogKill(___data.lastSourceOfDamage, ___data.player);
        }
    }
    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die")]
    class HealthHandler_Patch_RPCA_Die
    {
        static void Postfix(HealthHandler __instance, CharacterData ___data)
        {
            if (GameModeCollection.HideGunOnDeath)
            {
                // hide gun on death
                ___data?.weaponHandler?.gun?.gameObject?.SetActive(false);
            }
        }
        [HarmonyPriority(Priority.First)]
        static bool Prefix(HealthHandler __instance)
        {
            return !__instance.Invulnerable() && !__instance.Intangible() && (!__instance.isRespawning || GameModeManager.CurrentHandlerID != TRTHandler.GameModeID);
        }

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

    [HarmonyPatch(typeof(HealthHandler), "RPCA_Die")]
    class HealthHandler_Patch_RPCA_Die_Silence
    {
        /// patch to silence the death sound if the killing weapon was a silenced weapon

        static void PlayDeathHandleSilence(SoundManager soundManager, SoundEvent soundDie, Transform transform, HealthHandler healthHandler)
        {
            CharacterData data = (CharacterData)healthHandler.GetFieldValue("data");
            if (data?.lastSourceOfDamage?.data?.weaponHandler?.gun?.GetData()?.silenced ?? false)
            {
                // do not play death sound
                GameModeCollection.Log("[RPCA_Die Silence Patch] Silent death");
                return;
            }
            else
            {
                GameModeCollection.Log("[RPCA_Die Silence Patch] Normal death");
                soundManager.Play(soundDie, transform);
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_play = ExtensionMethods.GetMethodInfo(typeof(SoundManager), nameof(SoundManager.Play), new System.Type[] {typeof(SoundEvent), typeof(Transform)});
            var m_playHandleSilence = ExtensionMethods.GetMethodInfo(typeof(HealthHandler_Patch_RPCA_Die_Silence), nameof(PlayDeathHandleSilence));
            foreach (var code in instructions)
            {
                if (code.Calls(m_play))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, m_playHandleSilence);
                }
                else
                {
                    yield return code;
                }
            }
        }
    }
}
