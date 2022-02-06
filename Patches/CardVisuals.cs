using UnityEngine;
using HarmonyLib;
using UnboundLib;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CardVisuals), "Start")]
    class CardVisuals_Patch_Start
    {
        static bool Prefix(CardVisuals __instance)
        {
            if (__instance.GetComponentInParent<CardInfo>() == null)
            {
                __instance.SetFieldValue("group", __instance.transform.Find("Canvas/Front/Grid").GetComponent<CanvasGroup>());
                __instance.SetFieldValue("part", __instance.GetComponentInChildren<GeneralParticleSystem>());
                __instance.SetFieldValue("shake", __instance.GetComponent<ScaleShake>());
                __instance.SetFieldValue("cardAnims", __instance.GetComponentsInChildren<CardAnimation>());
                __instance.isSelected = false;
                __instance.ChangeSelected(false);
                return false;
            }
            return true;
        }
    }
}
