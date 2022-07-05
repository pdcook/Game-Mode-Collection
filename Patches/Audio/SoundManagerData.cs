using System;
using HarmonyLib;
using Sonigon;
using Sonigon.Internal;
using UnityEngine;
using System.Linq;
using GameModeCollection.Utils;
using UnboundLib;
using System.Collections.Generic;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(SoundManagerData), "PlaySoundEvent")]
    [HarmonyPriority(Priority.First)]
    public class SoundManagerPlaySoundEventPatch
    {
        public static bool IsStaticOrUI(Transform owner)
        {
            if (owner.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
            else if (owner == (Transform)SoundManager.Instance.InvokeMethod("GetMusicTransform") || owner == SoundManager.Instance.GetTransform())
            {
                return true;
            }
            else
            {
                foreach (Transform t in owner.GetComponentsInParent<Transform>().Reverse())
                {
                    if (t.name.Contains("UI") || t.name.Contains("Static") || t.name.ToLower().Contains("gamemode"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void Prefix(ref SoundEvent soundEvent, PlayType playType, Transform owner, Vector3? positionVector, Transform positionTransform, ref SoundParameterBase[] soundParameterArray)
        {
            if (!GameModeCollection.UseSpatialAudio) { return; }

            // if there is no position specified to play the audio at
            // and if the owner is on the UI layer, or has "UI" or "Static" in the name, then skip
            if (!(playType == PlayType.PlayAtVector && positionVector.HasValue) && owner?.gameObject != null && IsStaticOrUI(owner))
            {
                return;
            }

            if (soundParameterArray?.OfType<SoundParameterBypassSpatialize>().Any(s => s.bypassSpatialize) ?? false)
            {
                // if the sound has a sound parameter specifying that it should not be spatialized, then skip
                return;
            }

            if (playType == PlayType.PlayAtVector && positionVector.HasValue)
            {
                soundParameterArray = SoundManagerPatchPlaySoundEventHelper.PatchSoundParameterArray(soundParameterArray ?? new SoundParameterBase[0], positionVector);
            }
            else
            {
                soundParameterArray = SoundManagerPatchPlaySoundEventHelper.PatchSoundParameterArray(soundParameterArray ?? new SoundParameterBase[0], positionTransform);
            }
        }
    }
    static class SoundManagerPatchPlaySoundEventHelper
    {
        internal static SoundParameterBase[] PatchSoundParameterArray(SoundParameterBase[] original, Vector2? position)
        {
            if (!position.HasValue)
            {
                return original;
            }
            
            Player localPlayer = PlayerManager.instance.GetLocalPlayer();
            Vector2 listenerLoc = (localPlayer?.data?.dead ?? true) ? (Vector2)MainCam.instance.transform.position : localPlayer.data.playerVel.position;

            // get the parameters for the spatialization
            float minDistance = original.OfType<SoundParameterSpatializeMinDistance>().FirstOrDefault()?.root?.valueFloat ?? GMCAudio.MinDistance;
            float maxDistance = original.OfType<SoundParameterSpatializeMaxDistance>().FirstOrDefault()?.root?.valueFloat ?? GMCAudio.MaxDistance;
            float cutoffDistance = original.OfType<SoundParameterSpatializeCutoffDistance>().FirstOrDefault()?.root?.valueFloat ?? GMCAudio.CutoffDistance;
            AudioRolloffMode rolloff = (AudioRolloffMode) (original.OfType<SoundParameterSpatializeRolloff>().FirstOrDefault()?.root?.valueInt ?? (int)GMCAudio.Rolloff);
            float wallPenaltyPercent = original.OfType<SoundParameterSpatializeWalls>().FirstOrDefault()?.root?.valueFloat ?? GMCAudio.WallPenaltyPercent;
            int maxWallsCutoff = original.OfType<SoundParameterSpatializeWalls>().FirstOrDefault()?.root?.valueInt ?? GMCAudio.MaxWallsCutoff;

            float volume = Utils.GMCAudio.Falloff(listenerLoc, position.Value, rolloff, minDistance, maxDistance, cutoffDistance, wallPenaltyPercent, maxWallsCutoff);

            SoundParameterVolumeRatio soundParameterVolumeRatio = original.OfType<SoundParameterVolumeRatio>().FirstOrDefault();
            if (soundParameterVolumeRatio is null)
            {
                soundParameterVolumeRatio = new SoundParameterVolumeRatio(volumeRatio: volume);
                return original.Concat(new SoundParameterBase[] { soundParameterVolumeRatio }).ToArray();
            }
            else
            {
                int index = Array.IndexOf(original, soundParameterVolumeRatio);
                soundParameterVolumeRatio.volumeRatio *= volume;
                original[index] = soundParameterVolumeRatio;
                return original;
            }
        }
        internal static SoundParameterBase[] PatchSoundParameterArray(SoundParameterBase[] original, Transform positionTransform)
        {
            return PatchSoundParameterArray(original, positionTransform?.position);
        }
    }
}