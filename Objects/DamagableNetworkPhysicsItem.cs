using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace GameModeCollection.Objects
{
    public abstract class DamagableNetworkPhysicsItem<TCollision, TTrigger> : NetworkPhysicsItem<TCollision, TTrigger> where TCollision : Collider2D where TTrigger : Collider2D
    {
        public ObjectDamagable Damagable => this.gameObject.GetComponent<ObjectDamagable>();
        public ObjectHealthHandler Health => this.gameObject.GetComponent<ObjectHealthHandler>();
    }
}
