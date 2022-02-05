using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace GameModeCollection.Objects
{
    public abstract class DamagableNetworkPhysicsItem<TCollision, TTrigger> : NetworkPhysicsItem<TCollision, TTrigger> where TCollision : Collider2D where TTrigger : Collider2D
    {
        public ObjectDamagable Damagable => this.gameObject.GetComponent<ObjectDamagable>();
        public ObjectHealthHandler Health => this.gameObject.GetComponent<ObjectHealthHandler>();

        protected internal override void OnCollisionEnter2D(Collision2D collision)
        {
            ProjectileCollision projCol = collision?.collider?.GetComponent<ProjectileCollision>();
            if (projCol != null && projCol?.transform?.parent?.GetComponent<ProjectileHit>() != null && (projCol.transform.parent.GetComponentInChildren<PhotonView>().IsMine || PhotonNetwork.OfflineMode))
            {
                Vector2 point = (Vector2)projCol.transform.position;
                Vector2 damage = projCol.gameObject.GetComponentInParent<ProjectileHit>().dealDamageMultiplierr * (projCol.gameObject.GetComponentInParent<ProjectileHit>().bulletCanDealDeamage ? projCol.gameObject.GetComponentInParent<ProjectileHit>().damage : 1f) * (Vector2)projCol.transform.parent.forward;
                Player damagingPlayer = projCol.gameObject.GetComponentInParent<ProjectileHit>().ownPlayer;
                GameObject damagingWeapon = projCol.gameObject.GetComponentInParent<ProjectileHit>().ownWeapon;
                this.Damagable.CallTakeDamage(damage, point, damagingWeapon, damagingPlayer, true);
            }

            base.OnCollisionEnter2D(collision);
        }
    }
}
