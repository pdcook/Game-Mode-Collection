using System;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Swapper : Jester
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Swapper", 'S', GM_TRT.SwapperColor);

        public override TRT_Role_Appearance Appearance => Swapper.RoleAppearance;


        public override void OnKilledByPlayer(Player killingPlayer)
        {
            // do swapper stuff
        }
    }
}
