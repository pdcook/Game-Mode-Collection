using HarmonyLib;
using UnityEngine;
using UnboundLib;
using GameModeCollection.GameModes.TRT;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
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

            if (GameModeCollection.ForceEqualPlayerSize)
            {
                __instance.transform.localScale = Vector3.one * 1.2f * Mathf.Pow(1.2f, 0.2f);
                ___data.playerVel.SetFieldValue("mass", 100f * Mathf.Pow(1.2f, 0.8f));
            }
        }
    }
    [HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
    [HarmonyPriority(Priority.Last)]
    class CharacterStatModifiers_Patch_ResetStats
    {
        static float HealthToSet(CharacterData data)
        {
            float BaseHealth = 100f;
            if (data.GetComponentInChildren<ITRT_Role>() != null)
            {
                BaseHealth = data.GetComponentInChildren<ITRT_Role>().BaseHealth;
            }
            if (GameModeCollection.ReviveOnCardAdd)
            {
                return BaseHealth;
            }
            else
            {
                return BaseHealth * data.health / data.maxHealth;
            }
        }
        static float MaxHealthToSet(CharacterData data)
        {
            float BaseHealth = 100f;
            if (data.GetComponentInChildren<ITRT_Role>() != null)
            {
                BaseHealth = data.GetComponentInChildren<ITRT_Role>().BaseHealth;
            }
            return BaseHealth;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var f_health = ExtensionMethods.GetFieldInfo(typeof(CharacterData), nameof(CharacterData.health));
            var f_maxHealth = ExtensionMethods.GetFieldInfo(typeof(CharacterData), nameof(CharacterData.maxHealth));
            var f_data = ExtensionMethods.GetFieldInfo(typeof(CharacterStatModifiers), "data");
            var m_healthToSet = ExtensionMethods.GetMethodInfo(typeof(CharacterStatModifiers_Patch_ResetStats), nameof(HealthToSet));
            var m_maxHealthToSet = ExtensionMethods.GetMethodInfo(typeof(CharacterStatModifiers_Patch_ResetStats), nameof(MaxHealthToSet));
            int index = -1;
            int index2 = -1;

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].StoresField(f_health))
                {
                    index = i - 1;
                }
                if (codes[i].StoresField(f_maxHealth))
                {
                    index2 = i - 1;
                }
            }
            if (index == -1 || index2 == -1)
            {
                GameModeCollection.LogError("[CharacterStatModifiers.ResetStats] INSTRUCTION NOT FOUND");
            }
            else
            {
                codes[index2] = new CodeInstruction(OpCodes.Ldarg_0);
                codes.Insert(index2 + 1, new CodeInstruction(OpCodes.Ldfld, f_data));
                codes.Insert(index2 + 2, new CodeInstruction(OpCodes.Call, m_maxHealthToSet));

                codes[index] = new CodeInstruction(OpCodes.Ldarg_0);
                codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldfld, f_data));
                codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, m_healthToSet));
            }

            return codes.AsEnumerable();
        }
    }
}
