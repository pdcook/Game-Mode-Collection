using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Vampire : Traitor
    {
        new public static readonly TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Vampire", 'V', GM_TRT.VampireColor);

        public override TRT_Role_Appearance Appearance => Vampire.RoleAppearance;

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            // do vampire stuff
        }
    }
}
