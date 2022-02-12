using UnityEngine;
using UnboundLib;
using GameModeCollection.Objects;
namespace GameModeCollection.Extensions
{
    static class HealthHandlerExtensions
    {
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

        private class Corpse : PlayerDoNotFollow
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
