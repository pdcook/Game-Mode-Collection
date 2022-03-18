using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.Extensions;
using System.Collections;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class HypnotistRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Hypnotist.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Hypnotist.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.2f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Traitor;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Hypnotist>();
        }
    }

    public class Hypnotist : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Hypnotist", 'H', GM_TRT.HypnotistColor);

        public override TRT_Role_Appearance Appearance => Hypnotist.RoleAppearance;
        public override int StartingCredits => 0;

        private bool CanRevive = true;

        protected override void Start()
        {
            base.Start();

            this.CanRevive = true;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            if (interact)
            {
                // do hypnotist stuff
                if (!this.CanRevive || corpse.GetComponent<Player>() is null || !this.GetComponent<PhotonView>().IsMine) { return; }
                // cannot revive phantoms that have yet to respawn
                if (RoleManager.GetPlayerRoleID(corpse.Player) == PhantomRoleHandler.PhantomRoleID && ((Phantom)RoleManager.GetPlayerRole(corpse.Player)).IsHaunting)
                {
                    TRTHandler.SendChat(null, $"You cannot revive a corpse of a {RoleManager.GetRoleColoredName(Phantom.RoleAppearance)} that is currently haunting someone." , true);
                    return;
                }
                this.CanRevive = false;  
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_HypotistRevive), RpcTarget.All, corpse.GetComponent<Player>().playerID);
            }
            else
            {
                corpse.SearchBody(this.GetComponent<Player>(), false);
            }
        }
        [PunRPC]
        private void RPCA_HypotistRevive(int playerID)
        {
            this.CanRevive = false;
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            foreach (var role in player.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            GameModeCollection.instance.StartCoroutine(IDoHypnotistRevive(player));
        }
        IEnumerator IDoHypnotistRevive(Player player)
        {
            yield return new WaitForEndOfFrame();
            while (player.data.dead)
            {
                // delay the revive enough so that the player has been dead for at least GM_TRT.DelayRevivesFor to ensure network BS doesn't kill them again immediately
                TRT_Corpse corpse = player.GetComponent<TRT_Corpse>();
                float delayFor = GM_TRT.DelayRevivesFor;
                if (corpse != null && (Time.realtimeSinceStartup - corpse.TimeOfDeath) < GM_TRT.DelayRevivesFor)
                {
                    delayFor = GM_TRT.DelayRevivesFor - (Time.realtimeSinceStartup - corpse.TimeOfDeath);
                }
                player.data.healthHandler.Revive(true, delayReviveFor: delayFor);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitUntil(() => !player.data.dead && !player.data.healthHandler.Invulnerable() && !player.data.healthHandler.Intangible());
            yield return new WaitForEndOfFrame();
            RoleManager.GetHandler(TraitorRoleHandler.TraitorRoleID).AddRoleToPlayer(player);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            RoleManager.DoRoleDisplaySpecific(player);
            if (player.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"A {RoleManager.GetRoleColoredName(Hypnotist.RoleAppearance)} has revived you as a {RoleManager.GetRoleColoredName(Traitor.RoleAppearance)}!" , true);
            }
            yield break;
        }
    }
}
