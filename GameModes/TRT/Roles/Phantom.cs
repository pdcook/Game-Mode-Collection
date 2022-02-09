using System;
using UnityEngine;

namespace GameModeCollection.GameModes.TRT.Roles
{
    public class Phantom : Innocent
    {
        new public readonly static TRT_Role_Appearance RoleAppearance = new TRT_Role_Appearance("Phantom", 'P', GM_TRT.PhantomColor);

        public override TRT_Role_Appearance Appearance => Phantom.RoleAppearance;

        public override void OnKilledByPlayer(Player killingPlayer)
        {
            // do phantom stuff
        }
    }
}
