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
				return;
            }

			if (!(GeneralParticleSystem)instance.GetFieldValue("part"))
			{
				return;
			}
			if (instance.isSelected == setSelected)
			{
				return;
			}
			instance.isSelected = setSelected;
			Action<bool> action = instance.toggleSelectionAction;
			if (action != null)
			{
				action(instance.isSelected);
			}
			if (instance.isSelected)
			{
				((ScaleShake)instance.GetFieldValue("shake")).targetScale = 1.15f;
				return;
			}
			((ScaleShake)instance.GetFieldValue("shake")).targetScale = 0.9f;
		}
    }
}
