using UnboundLib;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Collections.Generic;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class AssassinRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Assassin.RoleAlignment;
        public string WinMessage => "TRAITORS WIN";
        public Color WinColor => Traitor.RoleAppearance.Color;
        public string RoleName => Assassin.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public int MinNumberOfPlayersWithRole => 0;
        public int MaxNumberOfPlayersWithRole => 1;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Assassin>();
        }
    }
    public class Assassin : Traitor
    {
        // specific to the assassin
        public const float TargetMultiplier = 2f;
        public const float NonTargetMultiplier = 0.5f;
        public Player Target { get; private set; }
        public bool FailedContract { get; private set; } = false;
        private bool HasBeenTold = false;

        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Assassin", 'A', GM_TRT.AssassinColor);

        public override TRT_Role_Appearance Appearance => Assassin.RoleAppearance;

        bool CanBeTarget(Player player)
        {
            Alignment? appearsAs = RoleManager.GetPlayerAlignmentAsSeenByOther(player, this.GetComponent<Player>());
            Alignment? trueAlignment = RoleManager.GetPlayerAlignment(player);
            if (trueAlignment is null) { return false; }
            return appearsAs != Alignment.Traitor && trueAlignment != Alignment.Chaos;
        }

        void SetNewTarget()
        {
            if (!this.GetComponent<PhotonView>().IsMine) { return; }

            Player target = null;

            if (!this.FailedContract)
            {
                List<Player> possibleTargets = PlayerManager.instance.players.Where(p => !p.data.dead && CanBeTarget(p) && RoleManager.GetPlayerRole(p)?.Appearance?.Name != Detective.RoleAppearance.Name).ToList();
                if (possibleTargets.Count() == 0)
                {
                    target = PlayerManager.instance.players.Find(p => !p.data.dead && RoleManager.GetPlayerRole(p)?.Appearance?.Name == Detective.RoleAppearance.Name);
                }
                else
                {
                    target = possibleTargets.OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).First();
                }
            }
            if (target == null && !this.HasBeenTold)
            {
                this.HasBeenTold = true;
                TRTHandler.SendChat(null, $"{RoleManager.GetRoleColoredName(this.Appearance)}, you have failed your contract.", true);
            }
            else if (target != null)
            {
                TRTHandler.SendChat(null, $"{RoleManager.GetRoleColoredName(this.Appearance)}, your target is {TRTHandler.GetPlayerColorNameAsColoredString(target)}.", true);
            }
            this.GetComponent<PhotonView>().RPC(nameof(RPCA_SetNewTarget), RpcTarget.All, target?.playerID);
        }
        [PunRPC]
        void RPCA_SetNewTarget(int? playerID)
        {
            this.Target = playerID is null ? null : PlayerManager.instance.players.Find(p => p.playerID == (int)playerID);
        }

        protected override void Start()
        {
            this.FailedContract = false;
            this.HasBeenTold = false;

            base.Start();

            this.ExecuteAfterFrames(2, () => this.SetNewTarget());
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do assassin stuff
            if (killedPlayer.playerID != this.Target?.playerID)
            {
                this.FailedContract = true;
            }
            this.SetNewTarget();
        }
    }
}
