using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Sonigon;

namespace GameModeCollection.Objects
{
    public class ObjectHealthHandler : MonoBehaviour
    {
        public Player LastSourceOfDamage { get; protected set; } = null;
        public float Health { get; protected set; }
        public float InvulnerableFor { get; protected set; } = 0f;
        public bool Dead { get; protected set; } = false;
        public float MaxHealth { get; set; } = 100f;

        protected PhotonView View => this.gameObject.GetComponent<PhotonView>();
        protected ObjectDamagable Damagable => this.gameObject.GetComponent<ObjectDamagable>();

        protected Action<Player> onPlayerKilledAction = null;
        protected float lastDamaged = -1f;

        public void SetInvulnerableFor(float time)
        {
            this.InvulnerableFor = time;
        }

        public void ResetPlayerKilledAction()
        {
            this.onPlayerKilledAction = null;
        }
        public void AddPlayerKilledAction(Action<Player> action)
        {
            if (this.onPlayerKilledAction is null)
            {
                this.onPlayerKilledAction = action;
            }
            else
            {
                this.onPlayerKilledAction += action;
            }
        }
        protected virtual void Update()
        {
            if (this.InvulnerableFor > 0f)
            {
                this.InvulnerableFor -= Time.deltaTime;
            }
        }
        public virtual void Revive()
        {
            this.Health = this.MaxHealth;
            this.Dead = false;
            this.LastSourceOfDamage = null;
        }
        public virtual void TakeDamage(Vector2 damage, Player damagingPlayer)
        {
            if (this.Dead || this.InvulnerableFor > 0f) { return; }
            this.Health -= damage.magnitude;
            this.LastSourceOfDamage = damagingPlayer;

            if (this.Health <= 0f && this.View.IsMine)
            {
                this.View.RPC(nameof(RPCA_Die), RpcTarget.All, damage, damagingPlayer.playerID);
            }

            if (damagingPlayer != null)
            {
                damagingPlayer.data.stats.DealtDamage(damage, false, null);
            }

            if (this.lastDamaged + 0.15f < Time.time && damagingPlayer != null && damagingPlayer.data.stats.lifeSteal != 0f)
            {
                SoundManager.Instance.Play(damagingPlayer.data.healthHandler.soundDamageLifeSteal, this.transform);
            }

            this.lastDamaged = Time.time;
        }
        [PunRPC]
        protected virtual void RPCA_Die(Vector2 deathDirection, int killingPlayerID)
        {
            this.Dead = true;
            this.Damagable.StopAllCoroutines();
            Player killingPlayer = PlayerManager.instance.players.Find(p => p.playerID == killingPlayerID);
            if (killingPlayer is null) { return; }
            this.onPlayerKilledAction?.Invoke(killingPlayer);
        }
    }
}
