using System;
using UnityEngine;
using HarmonyLib;
using GameModeCollection.Objects;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CardInfo),"Awake")]
    class CardInfo_Patch_Awake
    {
        static bool Prefix(CardInfo __instance)
        {
            if (__instance.GetComponent<CardItem>() != null) { return false; }
            return true;
        }
    }
}
