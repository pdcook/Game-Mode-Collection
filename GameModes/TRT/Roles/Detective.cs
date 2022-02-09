using UnityEngine;
namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Detective : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Detective", 'D', GM_TRT.DetectiveColor);
        public override TRT_Role_Appearance Appearance => Detective.RoleAppearance;

        public override bool AlertAlignment(Alignment alignment)
        {
            return true;
        }

        public override TRT_Role_Appearance AppearToAlignment(Alignment alignment)
        {
            return Detective.RoleAppearance;
        }

        public override void OnInteractWithCorpse(TRT_Corpse corpse)
        {
            GM_TRT.instance.IdentifyBody(corpse, true);
        }
    }
}
