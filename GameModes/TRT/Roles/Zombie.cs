using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Zombie : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Zombie", 'Z', GM_TRT.ZombieColor);

        public override TRT_Role_Appearance Appearance => Zombie.RoleAppearance;

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
        public override void OnKilledPlayer(Player killedPlayer)
        {
            // do zombie stuff
        }
    }
}
