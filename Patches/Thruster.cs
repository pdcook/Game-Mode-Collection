using GameModeCollection.Objects;
using UnityEngine;
using HarmonyLib;
using Photon.Pun;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Thruster), "FixedUpdate")]
    class Thruster_Patch_FixedUpdate
    {
        // patch to allow thrusters to interact with PhysicsItems properly
        static void Postfix(Thruster __instance, FollowLocalPos ___follow)
        {
            if (___follow?.target?.GetComponent<PhysicsItem>() != null)
            {
                PhysicsItem item = ___follow.target.GetComponent<PhysicsItem>();
                if (item.GetComponent<PhotonView>().IsMine || PhotonNetwork.OfflineMode)
                {
                    ___follow.target.GetComponent<PhysicsItem>().CallTakeForce((Vector2)__instance.transform.forward * __instance.force * __instance.physicsObjectM, (Vector2)__instance.transform.position, ForceMode2D.Force);
                }
            }
        }

    }
    [HarmonyPatch(typeof(Thruster), "Start")]
    class Thruster_Patch_Start
    {
        private const float VanillaDuration = 0.2f;
        // patch to allow thrusters to interact with PhysicsItems properly
        static void Postfix(Thruster __instance, FollowLocalPos ___follow)
        {
            if (___follow?.target?.GetComponent<PhysicsItem>() != null)
            {
                PhysicsItem item = ___follow.target.GetComponent<PhysicsItem>();

                ParticleSystem.MainModule mainModule = __instance.GetComponentInChildren<ParticleSystem>().main;
                mainModule.duration = mainModule.duration * item.PhysicalProperties.ThrusterDurationMult / VanillaDuration;

                __instance.GetComponent<DelayEvent>().time *= item.PhysicalProperties.ThrusterDurationMult / VanillaDuration;

                __instance.GetComponent<RemoveAfterSeconds>().seconds *= item.PhysicalProperties.ThrusterDurationMult / VanillaDuration;
            }
        }

    }
}
