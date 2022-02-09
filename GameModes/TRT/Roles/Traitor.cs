using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Traitor : TRT_Role
    {
        public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Traitor", 'T', GM_TRT.TraitorColor);

        public override TRT_Role_Appearance Appearance => Traitor.RoleAppearance;

        public override Alignment Alignment => Alignment.Traitor;

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;

        public override int StartingCards => 0;

        public override float BaseHealth => GM_TRT.BaseHealth;

        public override bool CanDealDamage => true;

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
                    return Traitor.RoleAppearance;
                case Alignment.Chaos:
                    return null;
                case Alignment.Killer:
                    return null;
                default:
                    return null;
            }
        }

        public override void OnCorpseInteractedWith(Player player)
        {
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            GM_TRT.instance.IdentifyBody(corpse, false);
        }

        public override void OnKilledByPlayer(Player killingPlayer)
        {
        }

        public override void OnKilledPlayer(Player killedPlayer)
        {
        }
    }
}
