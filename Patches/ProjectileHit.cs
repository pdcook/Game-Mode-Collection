using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnboundLib;
using GameModeCollection.Objects;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(ProjectileHit), nameof(ProjectileHit.RPCA_DoHit))]
    class ProjectileHit_Patch_RPCA_DoHit
    {
        static void CallBulletPush(ProjectileHit projectileHit, HitInfo hitInfo)
        {
            if (hitInfo?.collider == null || projectileHit == null) { return; }
            PhysicsItem item = hitInfo?.collider?.GetComponentInParent<PhysicsItem>();
            if (item != null && projectileHit.canPushBox)
            {
                item.BulletPush(projectileHit.transform.forward * projectileHit.force * 0.5f * projectileHit.damage * 100f, hitInfo.point, ((SpawnedAttack)projectileHit.GetFieldValue("spawnedAttack"))?.spawner?.data);
            }
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_bulletPush = ExtensionMethods.GetMethodInfo(typeof(NetworkPhysicsObject), nameof(NetworkPhysicsObject.BulletPush));
            var m_callBulletPush = ExtensionMethods.GetMethodInfo(typeof(ProjectileHit_Patch_RPCA_DoHit), nameof(CallBulletPush));

            int index = -1;
            var codes = instructions.ToList();
            for (int i = 3; i < codes.Count()-3; i++)
            {
                if (codes[i].Calls(m_bulletPush) && codes[i+1].opcode == OpCodes.Ldc_I4_0 && codes[i+2].opcode == OpCodes.Stloc_S)
                {
                    index = i + 3;
                    break;
                }
            }
            if (index == -1) { GameModeCollection.LogError("[ProjectileHit.RPCA_DoHit Patch] INSTRUCTION NOT FOUND"); }

            codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldloc_0));
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Call, m_callBulletPush));

            return codes.AsEnumerable();
        }
    }

    [HarmonyPatch(typeof(ProjectileHit), nameof(ProjectileHit.Hit))]
    [HarmonyPriority(Priority.First)]
    class ProjectileHitPatchHit
    {
        static int GetViewIDInParent(HitInfo hit, int viewID)
        {
            if (hit != null && hit.transform != null && hit.transform.GetComponent<PhysicsItem>() != null)
            {
                return hit.transform.GetComponent<PhotonView>().ViewID;
            }
            return viewID;

        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            var m_viewID = ExtensionMethods.GetPropertyInfo(typeof(PhotonView), nameof(PhotonView.ViewID));
            var m_getViewIDInParent = ExtensionMethods.GetMethodInfo(typeof(ProjectileHitPatchHit), nameof(GetViewIDInParent));

            int index = -1;

            for (int i = 1; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_0 && codes[i-1].Calls(m_viewID.GetMethod))
                {
                    index = i + 1;
                }
            }
            if (index == -1)
            {
                GameModeCollection.LogError("[ProjectileHit.Hit Patch] INSTRUCTION NOT FOUND");
            }
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_1));
            codes.Insert(index + 2, new CodeInstruction(OpCodes.Ldloc_0));
            codes.Insert(index + 3, new CodeInstruction(OpCodes.Call, m_getViewIDInParent));
            codes.Insert(index + 4, new CodeInstruction(OpCodes.Stloc_0));

            return codes.AsEnumerable();
        }

        // disable certain types of damage and hit effects depending on the gamemode
        private static bool Prefix(ProjectileHit __instance, HitInfo hit, bool forceCall)
        {
            HealthHandler healthHandler = null;
            if (hit?.transform)
            {
                healthHandler = hit.transform.GetComponent<HealthHandler>();
            }
            if (healthHandler)
            {
                Player hitPlayer = healthHandler.GetComponent<Player>();
                // if the hit player is not null
                if (hitPlayer != null && __instance?.ownPlayer != null)
                {
                    // self-damage
                    if (hitPlayer.playerID == __instance.ownPlayer.playerID && !GameModeCollection.SelfDamageAllowed)
                    {
                        return false;
                    }
                    // friendly fire
                    else if (hitPlayer.playerID != __instance.ownPlayer.playerID && hitPlayer.teamID == __instance.ownPlayer.teamID && !GameModeCollection.TeamDamageAllowed)
                    {
                        return false;
                    }
                    // enemy fire
                    else if (hitPlayer.playerID != __instance.ownPlayer.playerID && hitPlayer.teamID != __instance.ownPlayer.teamID && !GameModeCollection.EnemyDamageAllowed)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

}