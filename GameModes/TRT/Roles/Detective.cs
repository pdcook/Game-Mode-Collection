using UnityEngine;
using UnboundLib;
using UnboundLib.Utils;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class DetectiveRoleHandler : IRoleHandler
    {
        public static string DetectiveRoleName => Detective.RoleAppearance.Name;
        public static string DetectiveRoleID = $"GM_TRT_{DetectiveRoleName}";
        public const float DetectiveRarity = 0.125f;
        public Alignment RoleAlignment => Detective.RoleAlignment;
        public string WinMessage => "INNOCENTS WIN";
        public Color WinColor => Innocent.RoleAppearance.Color;
        public string RoleName => DetectiveRoleName;
        public string RoleID => DetectiveRoleID;
        public int MinNumberOfPlayersForRole => 0;
        public float Rarity => DetectiveRarity; // rarity is meaningless for Detective
        public string[] RoleIDsToOverwrite => new string[] { };
        public Alignment? AlignmentToReplace => null; // this is meaningless for Detective
        public void AddRoleToPlayer(Player player)
        {
            player.gameObject.GetOrAddComponent<Detective>();
        }
    }
    public class Detective : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance(Alignment.Innocent, "Detective", 'D', GM_TRT.DetectiveColor);
        public override TRT_Role_Appearance Appearance => Detective.RoleAppearance;
        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

        protected override void Start()
        {
            base.Start();

            CardInfo healingField = CardManager.cards.Values.First(card => card.cardInfo.name.Equals("Healing field")).cardInfo;

            // 80% of the time the detective spawns with healing field
            // 20% of the time they spawn with Golden Gun

        }

        public override bool AlertAlignment(Alignment alignment)
        {
            return false;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return Detective.RoleAppearance;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse, bool interact)
        {
            corpse.SearchBody(this.GetComponent<Player>(), true);
        }
    }
}
