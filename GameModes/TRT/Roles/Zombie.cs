using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.GameModeHandlers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class ZombieRoleHandler : IRoleHandler
    {
        public static string ZombieRoleName => Zombie.RoleAppearance.Name;
        public static string ZombieRoleID = $"GM_TRT_{ZombieRoleName}";
        public Alignment RoleAlignment => Zombie.RoleAlignment;
        public string WinMessage => "ZOMBIES WIN";
        public Color WinColor => Zombie.RoleAppearance.Color;
        public string RoleName => ZombieRoleName;
        public string RoleID => ZombieRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.1f;
        public string[] RoleIDsToOverwrite => new string[] { "GM_TRT_Traitor", "GM_TRT_Vampire", "GM_TRT_Hypnotist", "GM_TRT_Assassin" };
        public Alignment? AlignmentToReplace => Alignment.Traitor;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Zombie>();
        }
    }
    public class Zombie : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Zombie", 'Z', GM_TRT.ZombieColor);

        public override int MaxCards => 0;

        public override TRT_Role_Appearance Appearance => Zombie.RoleAppearance;

        private List<int> playerIDsKilled = new List<int>() { };

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Innocent:
                    return null;
                case Alignment.Traitor:
                    return Zombie.RoleAppearance;
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return null;
                default:
                    return null;
            }
        }
        protected override void Start()
        {
            base.Start();

            this.playerIDsKilled = new List<int>() { };

            // zombies cannot have cards, for now
            this.GetComponent<Player>()?.InvokeMethod("FullReset");
            if (this.GetComponent<PhotonView>().IsMine)
            {
                ModdingUtils.Utils.CardBarUtils.instance.PlayersCardBar(0).ClearBar();
            }
        }
        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do zombie stuff
            if (this.GetComponent<PhotonView>().IsMine && RoleManager.GetPlayerAlignment(killedPlayer) != this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>().playerID && !this.playerIDsKilled.Contains(killedPlayer.playerID))
            {
                // if this was the phantom and they can still haunt, don't revive them as a zombie
                if (RoleManager.GetPlayerRoleID(killedPlayer) != PhantomRoleHandler.PhantomRoleID || !(((Phantom)RoleManager.GetPlayerRole(killedPlayer)).CanHaunt || ((Phantom)RoleManager.GetPlayerRole(killedPlayer)).IsHaunting))
                {
                    this.playerIDsKilled.Add(killedPlayer.playerID);
                    this.GetComponent<PhotonView>().RPC(nameof(RPCA_ZombieInfect), RpcTarget.All, killedPlayer.playerID);
                }
            }

            base.OnKilledPlayer(killedPlayer);
        }
        [PunRPC]
        private void RPCA_ZombieInfect(int playerID)
        {
            Player player = PlayerManager.instance.players.Find(p => p.playerID == playerID);
            if (player is null) { return; }
            this.StartCoroutine(this.IDoZombieInfect(player));
        }
        private IEnumerator IDoZombieInfect(Player player)
        {
            yield return new WaitUntil(() => player.data.dead);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            foreach (var role in player.gameObject.GetComponentsInChildren<TRT_Role>())
            {
                UnityEngine.GameObject.Destroy(role);
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            this.GetComponent<PhotonView>().RPC(nameof(RPCA_ReviveZombie), RpcTarget.All, player.playerID);
            RoleManager.GetHandler(ZombieRoleHandler.ZombieRoleID).AddRoleToPlayer(player);
            RoleManager.DoRoleDisplaySpecific(player);
            if (player.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"You've been infected by a {RoleManager.GetRoleColoredName(Zombie.RoleAppearance)}!", true);
            }
        }
        [PunRPC]
        void RPCA_ReviveZombie(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            player.data.healthHandler.Revive(true);
        }
    }
}
