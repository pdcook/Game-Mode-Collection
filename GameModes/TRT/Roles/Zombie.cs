using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.Extensions;
using GameModeCollection.Utils;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class ZombieRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Zombie.RoleAppearance;
        public Alignment RoleAlignment => Zombie.RoleAlignment;
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] {Zombie.RoleAppearance};
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] { Innocent.RoleAppearance, Detective.RoleAppearance, Mercenary.RoleAppearance, Glitch.RoleAppearance, Phantom.RoleAppearance, Killer.RoleAppearance };
        public string WinCondition => $"Kill <b><i>or infect</i></b> all members of the {Innocent.RoleAppearance} team and the {Killer.RoleAppearance} if present.";
        public string Description =>
$@"Replaces all {Traitor.RoleAppearance}s for the round.

Spawns with <b>Claws</b>, a melee weapon which can be switched to with [Item 2].
    <b>Claws</b> deal 50 damage per slash.
    Killing a player with the <b>Claws</b> will infect them, and they will immediately respawn as a {Zombie.RoleAppearance}.

{Zombie.RoleAppearance}s <b><i>deal 50% damage with all non-Claw weapons.</i></b>

Has access to the {Zombie.RoleAppearance} shop.
    Spawns with no credits.
    Receives an additional credit each time they infect another player.";
    }
    public class ZombieRoleHandler : IRoleHandler
    {
        public static string ZombieRoleName => Zombie.RoleAppearance.Name;
        public static string ZombieRoleID = $"GM_TRT_{ZombieRoleName}";
        public IRoleHelp RoleHelp => new ZombieRoleHelp();
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
    public class Zombie : TRT_Role
    {
        public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Traitor, "Zombie", 'Z', GM_TRT.ZombieColor);
        public static readonly Alignment RoleAlignment = Alignment.Traitor;
        public override Alignment Alignment => Zombie.RoleAlignment;
        public override TRT_Role_Appearance Appearance => Zombie.RoleAppearance;

        private List<int> playerIDsKilled = new List<int>() { };
        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamageAndTakeEnvironmentalDamage => true;

        public override float KarmaChange { get; protected set; } = 0f;
        public override int StartingCredits => 0;

        public override bool AlertAlignment(Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Innocent:
                    return false;
                case Alignment.Traitor:
                    return true;
                case Alignment.Chaos:
                    return false;
                case Alignment.Killer:
                    return false;
                default:
                    return false;
            }
        }

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

            // zombies always spawn with claws
            if ((this.GetComponent<Player>()?.data?.view?.IsMine ?? false))
            {
                NetworkingManager.RPC(typeof(Zombie), nameof(RPCA_AddCardToPlayer), this.GetComponent<Player>().playerID);
            }

        }
        public override void TryShop()
        {
            TRTShopHandler.ToggleZombieShop(this.GetComponent<Player>());
        }

        [UnboundRPC]
        private static void RPCA_AddCardToPlayer(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return; }
            ModdingUtils.Utils.Cards.instance.AddCardToPlayer(player, ClawCard.Card, addToCardBar: false);
            if (player.data.view.IsMine)
            {
                CardUtils.ClientsideAddToCardBar(player.playerID, ClawCard.Card, silent: false);
            }
        }
        public void CallZombieInfect(Player killedPlayer)
        {
            // do zombie stuff
            if (this.GetComponent<PhotonView>().IsMine && RoleManager.GetPlayerAlignment(killedPlayer) != Alignment.Chaos && RoleManager.GetPlayerAlignment(killedPlayer) != this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>().playerID && !this.playerIDsKilled.Contains(killedPlayer.playerID))
            {
                // if this was the phantom and they can still haunt, don't revive them as a zombie
                if (RoleManager.GetPlayerRoleID(killedPlayer) != PhantomRoleHandler.PhantomRoleID || !(((Phantom)RoleManager.GetPlayerRole(killedPlayer)).CanHaunt || ((Phantom)RoleManager.GetPlayerRole(killedPlayer)).IsHaunting))
                {
                    this.playerIDsKilled.Add(killedPlayer.playerID);
                    this.GetComponent<PhotonView>().RPC(nameof(RPCA_ZombieInfect), RpcTarget.All, killedPlayer.playerID);
                    TRTShopHandler.GiveCreditToPlayer(this.GetComponent<Player>());
                }
            }
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
            while (player.data.dead)
            {
                player.data.healthHandler.Revive(true, delayReviveFor: GM_TRT.DelayRevivesFor);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitUntil(() => !player.data.dead && !player.data.healthHandler.Invulnerable() && !player.data.healthHandler.Intangible());
            yield return new WaitForEndOfFrame();
            RoleManager.GetHandler(ZombieRoleHandler.ZombieRoleID).AddRoleToPlayer(player);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            RoleManager.DoRoleDisplaySpecific(player);
            if (player.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"You've been infected by a {RoleManager.GetRoleColoredName(Zombie.RoleAppearance)}!", true);
            }
        }
        public override void OnCorpseInteractedWith(Player player)
        {
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // punish RDM
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == this.Alignment && killedPlayer?.playerID != this.GetComponent<Player>()?.playerID)
            {
                KarmaChange -= GM_TRT.KarmaPenaltyPerRDM;
            }
        }

        public override bool WinConditionMet(Player[] playersRemaining)
        {
            return playersRemaining.Count() == 0 || playersRemaining.Select(p => RoleManager.GetPlayerAlignment(p)).All(a => a == Alignment.Traitor || a == Alignment.Chaos);
        }
    }
}
