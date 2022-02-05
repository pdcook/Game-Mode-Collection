using HarmonyLib;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using GameModeCollection.Objects;
using Sonigon;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(DamageBox), "Collide")]
    class DamageBox_Patch_Collide
    {
        // patch to add force to physics items
        [HarmonyPrefix]
        static void Prefix_Force(DamageBox __instance, Collision2D collision)
        {
			if (Time.time < (float)__instance.GetFieldValue("time") + __instance.cd)
			{
				return;
			}
            if (collision?.transform?.GetComponent<PhysicsItem>() != null)
            {
                Vector3 vector = __instance.transform.root.forward;
                if (__instance.towardsCenterOfMap)
                {
                    vector = -collision.contacts[0].point.normalized;
                }
                if (__instance.awayFromMe)
                {
                    vector = (collision.transform.position - __instance.transform.position).normalized;
                }
                __instance.SetFieldValue("time", Time.time);
                PhysicsItem item = collision.transform.GetComponent<PhysicsItem>();
                item.CallTakeForce(__instance.damage * vector, collision.contacts[0].point, ForceMode2D.Impulse);
            }
        }
        // patch to prevent null reference exceptions for damagable physics objects
        [HarmonyPrefix]
        static bool Prefix_Damage(DamageBox __instance, Collision2D collision)
        {
			if (Time.time < (float)__instance.GetFieldValue("time") + __instance.cd)
			{
				return true;
			}
			Damagable componentInParent = collision.transform.GetComponentInParent<Damagable>();
			if (!componentInParent) { return true; }
			if (componentInParent && componentInParent.GetComponent<HealthHandler>() == null)
            {
				// this would usually throw a null reference exception in the original method
                if (componentInParent.GetComponent<ObjectDamagable>() == null)
                {
                    return false;
                }

                Vector3 vector = __instance.transform.root.forward;
                if (__instance.towardsCenterOfMap)
                {
                    vector = -collision.contacts[0].point.normalized;
                }
                if (__instance.awayFromMe)
                {
                    vector = (collision.transform.position - __instance.transform.position).normalized;
                }

                __instance.SetFieldValue("time", Time.time);

                componentInParent.CallTakeDamage(__instance.damage * vector, __instance.transform.position, null, ((SpawnedAttack)__instance.GetFieldValue("spawned") != null) ? ((SpawnedAttack)__instance.GetFieldValue("spawned")).spawner : null, true);
                if (__instance.soundPlaySawDamage)
                {
                    SoundManager.Instance.PlayAtPosition(__instance.soundSawDamage, SoundManager.Instance.GetTransform(), __instance.transform);
                }
                if (__instance.dmgPart)
                {
                    Vector3 forward = vector;
                    vector.z = 0f;
                    __instance.dmgPart.transform.parent.rotation = Quaternion.LookRotation(forward);
                    __instance.dmgPart.Play();
                }
                if (__instance.shake != 0f)
                {
                    NetworkingManager.RPC(typeof(DamageBox_Patch_Collide), nameof(RPCA_AllGameFeel), __instance.shake * vector);
                }

                return false;
            }
			return true;
		}
        [UnboundRPC]
        private static void RPCA_AllGameFeel(Vector2 vector)
        {
            GamefeelManager.instance.AddGameFeel(vector);
        }
        
    }
}
