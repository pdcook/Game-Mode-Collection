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
        public float Rarity => 0.2f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Traitor;
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

        private float timeUntilCheck = 1f;

        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Assassin", 'A', GM_TRT.AssassinColor);

        public override TRT_Role_Appearance Appearance => Assassin.RoleAppearance;
        public override int StartingCredits => 0;

        private List<int> playerIDsKilled = new List<int>() { };

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
                List<Player> possibleTargets = PlayerManager.instance.players.Where(p => !p.data.dead && CanBeTarget(p) && RoleManager.GetPlayerRoleID(p) != DetectiveRoleHandler.DetectiveRoleID).ToList();
                if (possibleTargets.Count() == 0)
                {
                    target = PlayerManager.instance.players.FirstOrDefault(p => !p.data.dead && RoleManager.GetPlayerRoleID(p) == DetectiveRoleHandler.DetectiveRoleID);
                }
                else
                {
                    target = possibleTargets.OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).First();
                }
            }
            else if (this.FailedContract && !this.HasBeenTold)
            {
                this.HasBeenTold = true;
                TRTHandler.SendChat(null, $"{RoleManager.GetRoleColoredName(this.Appearance)}, you have failed your contract.", true);
            }

            if (target != null)
            {
                TRTHandler.SendChat(null, $"{RoleManager.GetRoleColoredName(this.Appearance)}, your target is {TRTHandler.GetPlayerNameAsColoredString(target)}.", true);
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
            this.playerIDsKilled = new List<int>() { };

            base.Start();

            this.ExecuteAfterFrames(2, () => this.SetNewTarget());
        }

        void Update()
        {
            // if the target dies of means other than the assassin's, then get a new target - no penalty
            if (this.timeUntilCheck <= 0f)
            {
                this.timeUntilCheck = 1f;
                if (this.Target != null && this.Target.data.dead)
                {
                    this.SetNewTarget();
                }
            }
            this.timeUntilCheck -= Time.deltaTime;
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // for some dumb reason, OnKilledPlayer is called twice or more every time...
            if (this.GetComponent<PhotonView>().IsMine && !this.playerIDsKilled.Contains(killedPlayer.playerID))
            {
                this.playerIDsKilled.Add(killedPlayer.playerID);

                // do assassin stuff
                if (killedPlayer.playerID != this.Target?.playerID)
                {
                    this.FailedContract = true;
                }
                this.SetNewTarget();
            }

            base.OnKilledPlayer(killedPlayer);
        }
    }
}
