using System;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Innocent : TRT_Role
    {
        public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Innocent", 'I', GM_TRT.InnocentColor);

        public override TRT_Role_Appearance Appearance => Innocent.RoleAppearance;

        public override Alignment Alignment => Alignment.Innocent;

        public override int MaxCards => GM_TRT.BaseMaxCards;

        public override int StartingCards => 0;

        public override float BaseHealth => GM_TRT.BaseHealth;

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
            // punish RDM
            if (killedPlayer?.GetComponent<TRT_Role>()?.Alignment == Alignment.Innocent)
            {

            }
        }
    }
}
