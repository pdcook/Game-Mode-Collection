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
                    item.GetComponent<PhotonView>().RPC("RPCA_SendForce", RpcTarget.All, (Vector2)__instance.transform.forward * __instance.force * __instance.physicsObjectM, (Vector2)__instance.transform.position, (byte)ForceMode2D.Force);
                }
            }
        }

    }
}
