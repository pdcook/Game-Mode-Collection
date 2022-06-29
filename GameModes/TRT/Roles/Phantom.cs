using UnboundLib;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using GameModeCollection.GameModeHandlers;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class PhantomRoleHelp : IRoleHelp
    {
        public TRT_Role_Appearance RoleAppearance => Phantom.RoleAppearance;
        public Alignment RoleAlignment => Phantom.RoleAlignment;
        public TRT_Role_Appearance[] OpposingRoles => new TRT_Role_Appearance[] { Traitor.RoleAppearance, Hypnotist.RoleAppearance, Vampire.RoleAppearance, Killer.RoleAppearance };
        public TRT_Role_Appearance[] AlliedRoles => new TRT_Role_Appearance[] { Innocent.RoleAppearance, Glitch.RoleAppearance, Detective.RoleAppearance, Mercenary.RoleAppearance };
        public string WinCondition => $"Kill all members of the {Traitor.RoleAppearance} team and the {Killer.RoleAppearance} if present.";
        public string Description =>
$@"A special {Innocent.RoleAppearance}.

When the {Phantom.RoleAppearance} is killed, they will <b><i>haunt</i></b> their killer.
    <b><i>Haunted</i></b> players will constantly emit smoke.
    When a <b><i>haunted</i></b> player dies, the {Phantom.RoleAppearance} will respawn with 50% HP.
    The {Phantom.RoleAppearance} can only <b><i>haunt</i></b> once per round.

{Detective.RoleAppearance}s are notified when the {Phantom.RoleAppearance} is killed or revived.";
    }
    public class PhantomRoleHandler : IRoleHandler
    {
        public static string PhantomRoleName => Phantom.RoleAppearance.Name;
        public static string PhantomRoleID = $"GM_TRT_{PhantomRoleName}";
        public IRoleHelp RoleHelp => new PhantomRoleHelp();
        public Alignment RoleAlignment => Phantom.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => Phantom.RoleAppearance.Name;
        public string RoleID => PhantomRoleID;
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Phantom>();
        }
    }
    public class Phantom : Innocent
    {
        public const float ReviveWithHealthFrac = 0.5f;

        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Phantom", 'P', GM_TRT.PhantomColor);

        public bool CanHaunt { get; private set; } = true;
        public bool IsHaunting { get; internal set; } = false;

        public override TRT_Role_Appearance Appearance => Phantom.RoleAppearance;

        protected override void Start()
        {
            this.CanHaunt = true;
            this.IsHaunting = false;
            base.Start();
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            if (killingPlayer is null || killingPlayer.playerID == this.GetComponent<Player>()?.playerID) { return; }
            // do phantom stuff
            if (this.CanHaunt)
            {
                this.CanHaunt = false;
                this.IsHaunting = true;
                if (!this.GetComponent<PhotonView>().IsMine) { return; }
                this.GetComponent<PhotonView>().RPC(nameof(RPCA_HauntPlayer), RpcTarget.All, killingPlayer.playerID);
            }
        }
        [PunRPC]
        private void RPCA_HauntPlayer(int hauntedPlayerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == hauntedPlayerID);
            if (player is null) { return; }
            player.gameObject.AddComponent<PhantomHaunt>().SetPhantomPlayer(this);
            // if the local player is the detective, they should be notified that the phantom was killed
            if (RoleManager.GetPlayerRole(PlayerManager.instance.players.FirstOrDefault(p => p.data.view.IsMine))?.Appearance?.Name == Detective.RoleAppearance.Name)
            {
                TRTHandler.SendChat(null, $"The {RoleManager.GetRoleColoredName(this.Appearance)} has been killed!", true);
            }
        }
    }
}
