using UnityEngine;
using UnboundLib;
using GameModeCollection.Objects;
using System.Runtime.CompilerServices;
using System.Collections;
namespace GameModeCollection.Extensions
{
    public class HealthHandlerAdditionalData
    {
        public bool invulnerable = false; // take no damage
        public bool intangible = false; // bullets pass straight through
    }
    static class HealthHandlerExtensions
    {
        private static readonly ConditionalWeakTable<HealthHandler, HealthHandlerAdditionalData> additionalData = new ConditionalWeakTable<HealthHandler, HealthHandlerAdditionalData>();
        public static HealthHandlerAdditionalData GetData(this HealthHandler instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void SetInvulnerable(this HealthHandler instance, bool invulnerable)
        {
            instance.GetData().invulnerable = invulnerable;
        }
        public static void SetIntangible(this HealthHandler instance, bool intangible)
        {
            instance.GetData().intangible = intangible;
        }
        public static bool Invulnerable(this HealthHandler instance)
        {
            return instance.GetData().invulnerable;
        }
        public static bool Intangible(this HealthHandler instance)
        {
            return instance.GetData().intangible;
        }

        static private void TrySetEnabled<TComponent>(Component obj, bool enabled) where TComponent : Behaviour
        {
            if (obj != null && obj.GetComponent<TComponent>() != null)
            {
                obj.GetComponent<TComponent>().enabled = enabled;
            }
        }
        public static void MakeCorpse(this HealthHandler instance)
        {
            instance.PlayerCorpse(false);
        }
        public static void ReviveCorpse(this HealthHandler instance)
        {
            instance.PlayerCorpse(true);
        }

        public static void PlayerCorpse(this HealthHandler instance, bool revive = false)
        {
            if (instance == null) { return; }
            instance.gameObject?.GetComponent<Holding>()?.holdable?.gameObject?.SetActive(revive);
            instance.gameObject?.transform?.Find("WobbleObjects")?.gameObject?.SetActive(revive);
            instance.gameObject?.transform?.Find("Limbs")?.gameObject?.SetActive(revive);
            TrySetEnabled<GeneralInput>(instance, revive);
            TrySetEnabled<PlayerVelocity>(instance, revive);
            if (!revive) { instance.gameObject.GetOrAddComponent<Corpse>(); }
            else
            {
                if (instance.GetComponent<Corpse>() != null)
                {
                    GameObject.Destroy(instance.GetComponent<Corpse>());
                }
                instance.gameObject.transform.rotation = Quaternion.identity;
            }
        }

        // overload for reviving with other than full health, and adding a delay but still marking the player as alive immediately
        public static void Revive(this HealthHandler instance, bool isFullRevive = true, float healthPerc = 1f, float delayReviveFor = 0f)
        {
            instance.SetIntangible(true);
            instance.SetInvulnerable(true);
            ((CharacterData)instance.GetFieldValue("data")).dead = false;
            GameModeCollection.instance.StartCoroutine(DelayRevive(instance, isFullRevive, Mathf.Clamp01(healthPerc), Mathf.Clamp(delayReviveFor, 0f, float.PositiveInfinity)));
        }
        private static IEnumerator DelayRevive(HealthHandler instance, bool isFullRevive, float healthPerc, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            instance.SetIntangible(false);
            instance.SetInvulnerable(false);
            instance.Revive(isFullRevive);
            ((CharacterData)instance.GetFieldValue("data")).health *= healthPerc;
        }

        internal class Corpse : PlayerDoNotFollow
        {
            private const float AngularVelMult = 10f;
            private const float Drag = 10f;
            private const float AngularDrag = 10f;
            private const float Gravity = 3f;
            private Rigidbody2D Rig;
            void Start()
            {
                if (this.gameObject.GetComponent<Player>() == null)
                {
                    Destroy(this);
                }
                else
                {
                    this.Rig = this.gameObject.GetOrAddComponent<Rigidbody2D>();
                    this.Rig.isKinematic = false;
                    this.Rig.velocity = (Vector2)this.gameObject.GetComponent<PlayerVelocity>().GetFieldValue("velocity");
                    this.Rig.angularVelocity = (-AngularVelMult*((Vector2)this.gameObject.GetComponent<PlayerVelocity>().GetFieldValue("velocity")).x);
                    this.Rig.drag = Drag;
                    this.Rig.angularDrag = AngularDrag;
                    this.Rig.gravityScale = Gravity;
                    this.Rig.sharedMaterial = new PhysicsMaterial2D
                    {
                        bounciness = 0,
                        friction = 1
                    };
                }
            }
            void OnDestroy()
            {
                this.gameObject.transform.rotation = Quaternion.identity;
                Destroy(this.Rig);
            }
        }
    }
}
