namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Mercenary : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Mercenary", 'M', GM_TRT.MercenaryColor);

        public override TRT_Role_Appearance Appearance => Mercenary.RoleAppearance;

        public override int MaxCards => GM_TRT.BaseMaxCards + 1;
    }
}
