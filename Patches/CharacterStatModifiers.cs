using HarmonyLib;
using UnityEngine;
using UnboundLib;
using GameModeCollection.GameModes.TRT;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CharacterStatModifiers), "ConfigureMassAndSize")]
    [HarmonyPriority(Priority.Last)]
    class CharacterStatModifiers_Patch_ConfigureMassAndSize
    {
        static void Postfix(CharacterStatModifiers __instance, CharacterData ___data)
        {
            if (__instance.GetComponentInChildren<ITRT_Role>() != null)
            {
                float BaseHealth = __instance.GetComponentInChildren<ITRT_Role>().BaseHealth;
                __instance.transform.localScale = Vector3.one * 1.2f * Mathf.Pow(___data.maxHealth / BaseHealth * 1.2f, 0.2f) * __instance.sizeMultiplier;
                ___data.playerVel.SetFieldValue("mass", 100f * Mathf.Pow(___data.maxHealth / BaseHealth * 1.2f, 0.8f) * __instance.sizeMultiplier);
            }
        }
    }
    [HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
    [HarmonyPriority(Priority.Last)]
    class CharacterStatModifiers_Patch_ResetStats
    {
        static void Postfix(CharacterStatModifiers __instance, CharacterData ___data)
        {
            if (__instance.GetComponentInChildren<ITRT_Role>() != null)
            {
                float BaseHealth = __instance.GetComponentInChildren<ITRT_Role>().BaseHealth;
                ___data.health = BaseHealth;
                ___data.maxHealth = BaseHealth;
                __instance.WasUpdated();
                __instance.InvokeMethod("ConfigureMassAndSize");
            }
        }
    }
}
