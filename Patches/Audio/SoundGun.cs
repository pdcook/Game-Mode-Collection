using GameModeCollection.Extensions;
using HarmonyLib;
using SoundImplementation;
using UnboundLib;
using UnityEngine;
using GameModeCollection.Objects;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using Sonigon;
using Sonigon.Internal;
using System.Linq;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(SoundGun))]
    class SoundGunPatch_Extensions
    {
        static SoundParameterBase[] AddSoundParameters(SoundParameterBase[] original, SoundShotModifier shotModifier)
        {
            return original.AddRangeToArray(shotModifier.GetSoundParameterArray());
        }
        static SoundParameterBase[] AddSoundParameters(SoundParameterBase[] original, SoundImpactModifier impactModifier)
        {
            return original.AddRangeToArray(impactModifier.GetSoundParameterArray());
        }

        // patches to allow sound modifiers to have SoundParameter s
        static IEnumerable<CodeInstruction> MainPatch(IEnumerable<CodeInstruction> instructions, bool shot)
        {
            /// for PlaySingle/PlaySingleAutio/PlayShotgun/PlayShotgunAuto
            /* Original instructions        [stack]                         (notes)
             * 
             * ldc.i4.1                     [1]                             (load 1 onto the stack, as it is the size of the array about to be made)
             * newarr SoundParameterBase    [SoundParameterBase[1]]         (create a length-1 array of SoundParameterBase s)
             * dup                          [SoundParameterBase[1], SoundParameterBase[1]] (duplicates the array)
             * ldc.i4.0                     [0, SPB[1], SPB[1]]             (loads 0)
             * ldarg.0                      [this, 0, SPB[1], SPB[1]]       (loads the current instance)
             * ldfld SoundGun.soundParameterVolumeDecibleShot [soundParameterVDS, 0, SPB[1], SPB[1]] (loads the sound parameter to be stored in the array)
             * stelem.ref                   [SPB[1]{soundParameterVDS}]     (places the element on the top of the stack, the sound parameter) into the array at the third element on the stack at the position specified by the second element on the stack, here 0, overwriting anything that may already be there. the value, the index, AND the array are popped from the stack. hence why we needed dup earlier)
             * callvirt SoundManager.Play
             */

            /// for PlayImpactModifiers
            /* Original instructions        [stack]
             * 
             * ldc.i4.2                     [2]
             * newarr SoundParameterBase    [SoundParameterBase[2]]
             * dup                          [SoundParameterBase[2], SoundParameterBase[2]]
             * ldc.i4.0                     [0, SPB[2], SPB[2]]
             * ldarg.0                      [this, 0, SPB[2], SPB[2]]
             * ldfld SoundGun.soundParameterVolumeDecibleImpact [soundParameterVDI, 0, SPB[2], SPB[2]]
             * stelem.ref                   [SPB[2]{soundParameterVDI, null}]
             * dup                          [SPB[2]{soundParameterVDI, null}, SPB[2]{soundParameterVDI, null}]
             * ldc.i4.1                     [1, SPB[2], SPB[2]]
             * ldarg.0                      [this, 1, SPB[2], SPB[2]]
             * ldfld SoundGun.soundDamageToExplosionParameterIntensity [soundDTEPI, 1, SPB[2], SPB[2]]
             * stelem.ref                   [SPB[2]{soundParameterVDI, soundDTEPI}]
             * callvirt SoundManager.PlayAtPosition
             */


            /// After all of the above, right before callvirt, we want to
            /// concatenate the soundparameters from the extensions onto the sound parameter array

            /* New Instructions             [stack]                         (notes)
             * 
             */

            List<CodeInstruction> codes = instructions.ToList();

            var m_Play = ExtensionMethods.GetMethodInfo(typeof(SoundManager), nameof(SoundManager.Play), new System.Type[] { typeof(SoundEvent), typeof(Transform), typeof(SoundParameterBase[]) });
            var m_PlayAtPosition = ExtensionMethods.GetMethodInfo(typeof(SoundManager), nameof(SoundManager.PlayAtPosition), new System.Type[] { typeof(SoundEvent), typeof(Transform), typeof(Vector3), typeof(SoundParameterBase[]) });

            var f_SoundImpactModifierCurrentList = ExtensionMethods.GetFieldInfo(typeof(SoundGun), "soundImpactModifierCurrentList");
            var f_SoundShotModifierCurrentList = ExtensionMethods.GetFieldInfo(typeof(SoundGun), "soundShotModifierCurrentList");

            var m_AddShotParameters = ExtensionMethods.GetMethodInfo(typeof(SoundGunPatch_Extensions), nameof(AddSoundParameters), new System.Type[] {typeof(SoundParameterBase[]), typeof(SoundShotModifier)});
            var m_AddImpactParameters = ExtensionMethods.GetMethodInfo(typeof(SoundGunPatch_Extensions), nameof(AddSoundParameters), new System.Type[] {typeof(SoundParameterBase[]), typeof(SoundImpactModifier)});

            object m_getItem = codes.FirstOrDefault(c =>
            {
                return c.operand != null
                        && ((c.operand.ToString().Contains("SoundImpactModifier get_Item") && !shot)
                        || (c.operand.ToString().Contains("SoundShotModifier get_Item") && shot));
            })?.operand;
            if (m_getItem is null)
            {
                GameModeCollection.LogError("[SoundGunPatch_Extensions] Unable to find List<T>::get_Item method.");
                foreach (var code in codes)
                {
                    yield return code;
                }
                yield break;
            }

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].Calls(m_Play) || codes[i].Calls(m_PlayAtPosition))
                {

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    if (shot)
                    {
                        yield return new CodeInstruction(OpCodes.Ldfld, f_SoundShotModifierCurrentList);
                    }
                    else
                    {

                        yield return new CodeInstruction(OpCodes.Ldfld, f_SoundImpactModifierCurrentList);
                    }
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, m_getItem);

                    if (shot)
                    {
                        yield return new CodeInstruction(OpCodes.Call, m_AddShotParameters);
                    }
                    else
                    {

                        yield return new CodeInstruction(OpCodes.Call, m_AddImpactParameters);
                    }

                    yield return codes[i];

                }
                else
                {
                    yield return codes[i];
                }

            }

        }
        
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayImpactModifiers")]
        static IEnumerable<CodeInstruction> PlayImpactModifiers(IEnumerable<CodeInstruction> instructions)
        {
            return MainPatch(instructions, false);
        }
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgun")]
        static IEnumerable<CodeInstruction> PlayShotgun(IEnumerable<CodeInstruction> instructions)
        {
            return MainPatch(instructions, true);
        }
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgunAuto")]
        static IEnumerable<CodeInstruction> PlayShotgunAuto(IEnumerable<CodeInstruction> instructions)
        {
            return MainPatch(instructions, true);
        }
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingle")]
        static IEnumerable<CodeInstruction> PlaySingle(IEnumerable<CodeInstruction> instructions)
        {
            return MainPatch(instructions, true);
        }
        [HarmonyTranspiler]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingleAuto")]
        static IEnumerable<CodeInstruction> PlaySingleAuto(IEnumerable<CodeInstruction> instructions)
        {
            return MainPatch(instructions, true);
        }
    }
    [HarmonyPatch(typeof(SoundGun))]
    class SoundGunPatch_Silence
    {
        /// <summary>
        /// patches for silenced weapons
        /// </summary>

        const float eps = 0.1f;

        static bool CanSeePlayer(Player player, Vector2 position)
        {

            RaycastHit2D[] array = Physics2D.RaycastAll(position, (player.data.playerVel.position - position).normalized, Vector2.Distance(position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].transform
                    && !array[i].transform.root.GetComponent<SpawnedAttack>()
                    && !array[i].transform.root.GetComponent<Player>()
                    && !array[i].transform.root.GetComponentInChildren<PhysicsItem>()
                    && !(Vector2.Distance(array[i].point, position) < eps)
                    )
                {
                    return false;
                }
            }
            return true;
        }

        static bool ShouldPlaySound(SoundGun soundGun, Vector3? position = null)
        {
            Gun gun = (Gun)soundGun.GetFieldValue("parentGun");
            if (gun is null || !gun.GetData().silenced)
            {
                // if the gun is not silenced, or if the gun is null, just play the sound normally
                return true;
            }
            else
            {
                Vector3 soundPos = position ?? gun.transform.position;
                // if the gun is silenced, check to see if the local player has LoS
                Player localPlayer = PlayerManager.instance.GetLocalPlayer();
                if (localPlayer is null || localPlayer.data.dead || CanSeePlayer(localPlayer, soundPos))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(SoundGun.PlayImpact))]
        static bool PlayImpact(SoundGun __instance, HitInfo hit)
        {
            return ShouldPlaySound(__instance, hit.point);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayImpactModifiers")]
        static bool PlayImpactModifiers(SoundGun __instance, Vector2 position)
        {
            return ShouldPlaySound(__instance, position);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(nameof(SoundGun.PlayShot))]
        static bool PlayShot(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgun")]
        static bool PlayShotgun(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlayShotgunAuto")]
        static bool PlayShotgunAuto(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingle")]
        static bool PlaySingle(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch("PlaySingleAuto")]
        static bool PlaySingleAuto(SoundGun __instance)
        {
            return ShouldPlaySound(__instance);
        }
    }
}
