using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Hypnotist : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Hypnotist", 'H', GM_TRT.HypnotistColor);

        public override TRT_Role_Appearance Appearance => Hypnotist.RoleAppearance;

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            // do hypnotist stuff
        }
    }
}
