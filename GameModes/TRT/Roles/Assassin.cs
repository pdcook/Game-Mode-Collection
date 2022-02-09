using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Assassin : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Assassin", 'A', GM_TRT.AssassinColor);

        public override TRT_Role_Appearance Appearance => Assassin.RoleAppearance;

        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do assassin stuff
        }
    }
}
