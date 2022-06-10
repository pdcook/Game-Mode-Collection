using UnityEngine;
using UnboundLib;
using System;
namespace GameModeCollection.Extensions
{
    public static class CardVisualsExtensions
    {
        // overload for `ChangeSelected` to allow the size to change without flipping the card
        public static void ChangeSelected(this CardVisuals instance, bool setSelected, bool local)
        {
			if (local)
            {
				instance.ChangeSelected(setSelected);
            }
            // always set the scale to 1 to prevent weird physics interactions for non-host players
			((ScaleShake)instance.GetFieldValue("shake")).targetScale = 1f;
		}
    }
}
