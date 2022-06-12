using UnboundLib;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.Extensions;
using System.Collections;
using GameModeCollection.GameModes.TRT.RoundEvents;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class SwapperRoleHandler : IRoleHandler
    {
        public static string SwapperRoleName => Swapper.RoleAppearance.Name;
        public static string SwapperRoleID = $"GM_TRT_{SwapperRoleName}";
        public Alignment RoleAlignment => Swapper.RoleAlignment;
        public string WinMessage => "THE SWAPPER WINS"; // this shouldn't even be possible
        public Color WinColor => Swapper.RoleAppearance.Color;
        public string RoleName => SwapperRoleName;
        public string RoleID => SwapperRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] {};
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Swapper>();
        }
    }
    public class Swapper : Jester
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Chaos, "Swapper", 'S', GM_TRT.SwapperColor);

        public override TRT_Role_Appearance Appearance => Swapper.RoleAppearance;

        public bool HasSwapped { get; private set; } = false;

        protected override void Start()
        {
            this.HasSwapped = false;

            base.Start();
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            if (this.HasSwapped || killingPlayer?.playerID == this.GetComponent<Player>().playerID)
            {
                return;
            }
            this.HasSwapped = true;
            // get role of killing player
            ITRT_Role killingRole = RoleManager.GetPlayerRole(killingPlayer);
            if (killingRole is null) { return; }
            if (this.GetComponent<PhotonView>().IsMine)
            {
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_Swapped), RpcTarget.All, killingPlayer.playerID);
            }
        }
        public override bool WinConditionMet(Player[] playersRemaining)
        {
            // the swapper cannot win
            return false;
        }
        [PunRPC]
        private void RPCA_Swapped(int swappedID)
        {
            Player killingPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == swappedID);
            if (killingPlayer is null) { return; }
            ITRT_Role killingPlayerRole = RoleManager.GetPlayerRole(killingPlayer);
            string roleID = RoleManager.GetRoleID(killingPlayerRole);
            IRoleHandler roleHandler = RoleManager.GetHandler(roleID);

            // log the swap
            RoundSummary.LogEvent(SwapEvent.ID, this.GetComponent<Player>().playerID, killingPlayerRole.Appearance);
            RoundSummary.LogEvent(KilledChaosEvent.ID, swappedID, Swapper.RoleAppearance);

            GameModeCollection.instance.StartCoroutine(Swapper.IDoSwapFromSwapper(this.GetComponent<Player>(), roleHandler, killingPlayerRole));
            GameModeCollection.instance.StartCoroutine(Swapper.IDoSwapToSwapper(killingPlayer));

        }
        static IEnumerator IDoSwapFromSwapper(Player swapper, IRoleHandler killingPlayerRoleHandler, ITRT_Role killingPlayerRole)
        {
            // wait until the swapper has registered as dead
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => swapper.data.dead);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // the swapper has all of their roles removed
            foreach (TRT_Role role in swapper.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            // the swapper is revived
            while (swapper.data.dead)
            {
                swapper.data.healthHandler.Revive(true, delayReviveFor: GM_TRT.DelayRevivesFor); 
                yield return new WaitForEndOfFrame();
            }

            // wait until the swapper has revived
            yield return new WaitUntil(() => !swapper.data.dead && !swapper.data.healthHandler.Invulnerable() && !swapper.data.healthHandler.Intangible());
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // the swapper now assumes the role of the killing player
            killingPlayerRoleHandler.AddRoleToPlayer(swapper);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // finally, TRT chats and role displays are handled
            RoleManager.DoRoleDisplaySpecific(swapper);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            if (swapper.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"You've swapped to become a {RoleManager.GetRoleColoredName(killingPlayerRole.Appearance)}!", true);
            }
        }
        static IEnumerator IDoSwapToSwapper(Player killingPlayer)
        {
            // the killing player has all their roles removed and is killed
            foreach (var role in killingPlayer.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            if (killingPlayer.GetComponent<PhotonView>().IsMine)
            {
                killingPlayer.data.view.RPC("RPCA_Die", RpcTarget.All, Vector2.up);
            }

            yield return new WaitUntil(() => killingPlayer.data.dead);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // the (now killed) killing player will now appear as a swapper
            RoleManager.GetHandler(SwapperRoleHandler.SwapperRoleID).AddRoleToPlayer(killingPlayer);

            yield break;
        }
    }
}
