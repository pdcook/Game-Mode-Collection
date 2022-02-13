using System.Linq;
using UnityEngine;
using Photon.Pun;
using Sonigon;
using System.Collections;
using UnboundLib.GameModes;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;

namespace GameModeCollection.Objects
{
    [RequireComponent(typeof(PhotonView))]
    public class ObjectDamagable : Damagable
    {
        private PhotonView View => this.gameObject.GetComponent<PhotonView>();
        private ObjectHealthHandler Health => this.gameObject.GetComponent<ObjectHealthHandler>();

        public override void CallTakeDamage(Vector2 damage, Vector2 damagePosition, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            if (damage == Vector2.zero)
            {
                return;
            }
            this.View.RPC(nameof(RPCA_TakeDamage), RpcTarget.All, new object[]
            {
                damage,
                damagePosition,
                lethal,
                (damagingPlayer != null) ? damagingPlayer.playerID : -1
            });
        }

        public override void TakeDamage(Vector2 damage, Vector2 damagePosition, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true, bool ignoreBlock = false)
        {
            if (damage == Vector2.zero) { return; }
            this.TakeDamage(damage, damagePosition, damagingPlayer?.GetTeamColors()?.color ?? Color.red, damagingWeapon, damagingPlayer, lethal, ignoreBlock);
        }

        public override void TakeDamage(Vector2 damage, Vector2 damagePosition, Color dmgColor, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true, bool ignoreBlock = false)
        {
            /// Specifically for TRT
            if (GameModeManager.CurrentHandlerID == TRTHandler.GameModeID && !(damagingPlayer?.GetComponent<ITRT_Role>()?.CanDealDamage ?? true))
            {
                damage = Vector2.zero;
            }
            ///

            this.Health.TakeDamage(damage, damagingPlayer);
            foreach (PlayerSkinParticle skin in this.GetComponentsInChildren<PlayerSkinParticle>())
            {
                skin.BlinkColor(dmgColor);
            }
        }
        public virtual void TakeDamageOverTime(Vector2 damage, Vector2 position, float time, float interval, Color color, SoundEvent soundDamageOverTime, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            this.StartCoroutine(this.DoDamageOverTime(damage, position, time, interval, color, soundDamageOverTime, damagingWeapon, damagingPlayer, lethal));
        }
        protected virtual IEnumerator DoDamageOverTime(Vector2 damage, Vector2 position, float time, float interval, Color color, SoundEvent soundDamageOverTime, GameObject damagingWeapon = null, Player damagingPlayer = null, bool lethal = true)
        {
            float damageDealt = 0f;
            float damageToDeal = damage.magnitude;
            float dpt = damageToDeal / time * interval;
            while (damageDealt < damageToDeal)
            {
                if (soundDamageOverTime != null && !this.Health.Dead)
                {
                    SoundManager.Instance.Play(soundDamageOverTime, this.transform);
                }
                damageDealt += dpt;
                this.TakeDamage(damage.normalized * dpt, position, color, damagingWeapon, damagingPlayer, lethal, false);
                yield return new WaitForSeconds(interval / TimeHandler.timeScale);
            }
            yield break;
        }
        [PunRPC]
        protected virtual void RPCA_TakeDamage(Vector2 damage, Vector2 position, bool lethal = true, int playerID = -1)
        {
            if (damage == Vector2.zero)
            {
                return;
            }
            Player playerWithID = PlayerManager.instance.players.Where(p => p.playerID == playerID).FirstOrDefault();
            GameObject damagingWeapon = null;
            if (playerWithID != null)
            {
                damagingWeapon = playerWithID.data.weaponHandler.gun.gameObject;
            }
            this.TakeDamage(damage, position, damagingWeapon, playerWithID, lethal, true);
        }
    }
}
