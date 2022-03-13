using UnboundLib;
using UnityEngine;
using Photon.Pun;
using GameModeCollection.Objects;
using GameModeCollection.GameModes.TRT.Cards;
using UnboundLib.Networking;
using System.Linq;
using GameModeCollection.Utils;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class MercenaryRoleHandler : IRoleHandler
    {
        public Alignment RoleAlignment => Mercenary.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => Mercenary.RoleAppearance.Name;
        public string RoleID => $"GM_TRT_{this.RoleName}";
        public int MinNumberOfPlayersForRole => 5;
        public float Rarity => 0.25f;
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => Alignment.Innocent;
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Mercenary>();
        }
    }
    public class Mercenary : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Mercenary", 'M', GM_TRT.MercenaryColor);

        public override TRT_Role_Appearance Appearance => Mercenary.RoleAppearance;

        public override int StartingCredits => 1;

        protected override void Start()
        {
            base.Start();
        }
        public override void TryShop()
        {
            TRTShopHandler.ToggleTDShop(this.GetComponent<Player>());
        }
    }
}
