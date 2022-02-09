using System;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Killer : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Killer", 'K', GM_TRT.KillerColor);

        public override TRT_Role_Appearance Appearance => Killer.RoleAppearance;

        public override Alignment Alignment => Alignment.Killer;

        public override int MaxCards => GM_TRT.BaseMaxCards + 3;

        public override int StartingCards => 1;

        public override float BaseHealth => 1.5f*GM_TRT.BaseHealth;

        public override bool CanDealDamage => true;

        public override bool AlertAlignment(Alignment alignment)
        {
            return false;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return null;
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
