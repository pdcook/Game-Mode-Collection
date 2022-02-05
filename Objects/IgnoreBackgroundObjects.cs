using UnityEngine;
using UnboundLib;
namespace GameModeCollection.Objects
{
    class IgnoreBackgroundObjects : MonoBehaviour 
    {
        public readonly int LayerToIgnore = LayerMask.NameToLayer("BackgroundObject");

        private const float OverSize = 2f;
        private Collider2D ColliderToTrack = null;
        public BoxCollider2D Trigger { get; private set; }
        public void SetColliderToTrack(Collider2D collider)
        {
            this.ColliderToTrack = collider;
        }
        void Start()
        {
            this.gameObject.layer = LayerToIgnore;

            this.Trigger = this.gameObject.GetOrAddComponent<BoxCollider2D>();
            this.Trigger.isTrigger = true;
            this.Trigger.transform.localScale = Vector3.one;
        }
        void FixedUpdate()
        {
            if (this.ColliderToTrack == null) { return; }
            this.Trigger.size = OverSize * this.ColliderToTrack.bounds.size;
            this.Trigger.transform.position = this.ColliderToTrack.transform.position;
        }
		void OnTriggerEnter2D(Collider2D collider2D)
        {
            // if the collider is on the layer to ignore, then have all the colliders on the parent of this object ignore the collider
            if (collider2D.gameObject.layer == LayerToIgnore)
            {
                foreach (Collider2D collider in this.gameObject.transform.parent.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    Physics2D.IgnoreCollision(collider2D, collider, true);
                }
            }
            // even if it's not on the layer to ignore, have this trigger ignore it so we don't do all this logic again unnecessarily
            Physics2D.IgnoreCollision(collider2D, this.Trigger, true);
        }
    }
}
