using HarmonyLib;
using Sonigon.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
namespace GameModeCollection.Patches
{
    internal static class SoundParameterTypeExt
    {
        public static SoundParameterType GetSoundParameterType(string name)
        {
            try
            {
                return (SoundParameterType)SoundParameterTypes.Keys.First(i => SoundParameterTypes[i].ToString() == name);
            }
            catch (Exception e)
            {
                GameModeCollection.LogError($"[SoundParameterTypeExt] SoundParameterType {name} not found! Full exception: " + e.Message);
                return (SoundParameterType)SoundParameterTypes.Keys.FirstOrDefault(i => SoundParameterTypes[i].ToString() == "None");
            }
        }
        public static readonly Dictionary<int, string> SoundParameterTypes = new Dictionary<int, string>()
        {
            {0, "Volume"},
            {1, "Pitch"},
            {2, "Delay"},
            {3, "Increase2d"},
            {4, "Intensity"},
            {5, "ReverbZoneMix"},
            {6, "StartPosition"},
            {7, "Reverse"},
            {8, "StereoPan"},
            {9, "Polyphony"},
            {10, "DistanceScale"},
            {11, "DistortionIncrease"},
            {12, "FadeInLength"},
            {13, "FadeInShape"},
            {14, "FadeOutLength"},
            {15, "FadeOutShape"},
            {16, "FollowTransform"},
            {17, "BypassReverbZones"},
            {18, "BypassVoiceEffects"},
            {19, "BypassListenerEffects"},
            // custom types below
            {20, "None"},
            {21, "BypassSpatialize"},
            {22, "SpatializeWalls"},
            {23, "SpatializeMinDistance"},
            {24, "SpatializeMaxDistance"},
            {25, "SpatializeCutoffDistance"},
            {26, "SpatializeRolloff"},
        };
    }

    [HarmonyPatch(typeof(Enum), "GetValues")]
    internal class SoundParameterEnumValues
    {
        private static void Postfix(Type enumType, ref Array __result)
        {
            if (enumType == typeof(SoundParameterType))
            {
                __result = SoundParameterTypeExt.SoundParameterTypes.Keys.ToArray();
            }
        }
    }
    [HarmonyPatch(typeof(Enum), "GetNames")]
    internal class RarityEnumNames
    {
        private static void Postfix(Type enumType, ref string[] __result)
        {
            if (enumType == typeof(SoundParameterType))
            {
                __result = SoundParameterTypeExt.SoundParameterTypes.Values.ToArray();
            }
        }
    }
    [HarmonyPatch(typeof(Enum), "ToString", new Type[] { })]
    internal class RarityEnumToString
    {
        private static void Postfix(Enum __instance, ref string __result)
        {
            if (__instance.GetType() == typeof(SoundParameterType))
            {
                try
                {
                    __result = Enum.GetNames(typeof(SoundParameterType))[(int)(SoundParameterType)__instance];
                }
                catch (Exception e)
                {
                    GameModeCollection.LogError("[SoundParameterType Extension Patch] Error in SoundParameterType.ToString: " + e.Message);

                }
            }
        }
    }
}
