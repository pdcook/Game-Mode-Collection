using HarmonyLib;
using GameModeCollection.Objects;
using UnityEngine;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CharacterData), nameof(CharacterData.TouchGround))]
    class CharacterData_Patch_TouchGround
    {
        static void Postfix(CharacterData __instance, Rigidbody2D groundRig)
        {
            if (__instance != null && groundRig?.GetComponent<PlayerDoNotFollow>() != null)
            {
                __instance.standOnRig = null;
            }
        }
    }
}
