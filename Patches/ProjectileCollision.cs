using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnboundLib;
using GameModeCollection.Objects;
using GameModeCollection.Extensions;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(ProjectileCollision), "Die")]
    class ProjectileCollision_Patch_Die
    {
        static void Postfix(ProjectileCollision __instance)
        {
            if (__instance.GetComponentInParent<ProjectileHit>()?.ownWeapon?.GetComponent<Gun>()?.GetData().pierce ?? false)
            {
                __instance.SetFieldValue("hasCollided", false);
            }
        }
    }        
}