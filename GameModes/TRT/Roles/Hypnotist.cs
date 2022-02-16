﻿using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
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
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.15f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Hypnotist>();
        }
    }

    public class Hypnotist : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Hypnotist", 'H', GM_TRT.HypnotistColor);

        public override TRT_Role_Appearance Appearance => Hypnotist.RoleAppearance;

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
                if (!this.CanRevive || corpse.GetComponent<Player>() is null || !this.GetComponent<PhotonView>().IsMine) { return; }
                this.CanRevive = false;  
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_HypotistRevive), RpcTarget.All, corpse.GetComponent<Player>().playerID);
            }
            else
            {
                // do hypnotist stuff
                corpse.SearchBody(this.GetComponent<Player>(), false);
            }
        }
        [PunRPC]
        private void RPCA_HypotistRevive(int playerID)
        {
            this.CanRevive = false;
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            player.data.healthHandler.Revive(true);
            foreach (var role in player.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            this.ExecuteAfterFrames(2, () =>
            {
                RoleManager.GetHandler(TraitorRoleHandler.TraitorRoleID).AddRoleToPlayer(player);
                RoleManager.DoRoleDisplaySpecific(player);
                if (player.data.view.IsMine)
                {
                    TRTHandler.SendChat(null, $"A {RoleManager.GetRoleColoredName(Hypnotist.RoleAppearance)} has revived you as a {RoleManager.GetRoleColoredName(Traitor.RoleAppearance)}!" , true);
                }
            });
        }
    }
}