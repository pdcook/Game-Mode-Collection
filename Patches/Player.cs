using HarmonyLib;
using GameModeCollection.GameModes.TRT.Cards;
using UnityEngine;
using GameModeCollection.Extensions;
using System.Linq;
using UnboundLib;
using Photon.Pun;

namespace GameModeCollection.Patches
{
        [HarmonyPatch(typeof(Player), "FullReset")]
    class PlayerPatchFullReset
    {
        static void Prefix(Player __instance)
        {

            // hide claw just to be sure
            try
            {
                A_Claw.MakeGunClaw(__instance.playerID, false);
            }
            catch { }
            // hide knife just to be sure
            try
            {
                A_Knife.MakeGunKnife(__instance.playerID, false);
            }
            catch { }
            try
            {
                if (__instance.gameObject.GetComponent<GoldenDeagleGun>() != null)
                {
                    GameObject.Destroy(__instance.gameObject.GetComponent<GoldenDeagleGun>());
                }
            }
            catch { }
            try
            {
                if (__instance.gameObject.GetComponent<RifleGun>() != null)
                {
                    GameObject.Destroy(__instance.gameObject.GetComponent<RifleGun>());
                }
            }
            catch { }
            try
            {
                __instance.data.weaponHandler.gun.GetData().silenced = false;
            }
            catch { }
        }
    }
    [HarmonyPatch(typeof(Player), "Start")]
    class PlayerPatchStart
    {
        static void Postfix(Player __instance)
        {
            __instance.gameObject.GetOrAddComponent<SuffocationHandler>();
        }
    }
    class SuffocationHandler : MonoBehaviour
    {
        public const float SuffocationDamage = 5f;

        public const float SuffocationDelay = 0.5f;
        float suffocationTimer = 0f;
        Player player;
        void Start()
        {
            this.player = this.GetComponent<Player>(); 
        }
        void Update()
        {
            if (!GameModeCollection.SuffocationDamageEnabled)
            {
                return;
            }
            if (this.suffocationTimer > 0f)
            {
                this.suffocationTimer -= Time.deltaTime;
                return;
            }
            if (this.player.data.dead || !this.player.data.isPlaying || !(bool)this.player.data.playerVel.GetFieldValue("simulated"))
            {
                return;
            }
            // use an OverlapPoint to see if any objects on the default layer are overlaping the player
            // if so, and the object is part of the map, then the player is suffocating
            Collider2D[] colliders = Physics2D.OverlapPointAll(this.transform.position, 1 << LayerMask.NameToLayer("Default"));
            if (colliders.Any(c => c.transform.root.GetComponent<Map>() != null))
            {
                this.suffocationTimer = SuffocationDelay;
                if (this.GetComponent<PhotonView>().IsMine)
                {
                    this.player.data.healthHandler.CallTakeDamage(SuffocationDamage * Vector2.up, this.transform.position, null, null, true);
                }
            }
        }
    }

}
