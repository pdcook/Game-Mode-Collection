using HarmonyLib;
using UnityEngine;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CharacterItem),"Start")]
    class CharacterItem_Patch_Start
    {
		private static readonly Vector3 defaultHealthbarOffset = new Vector3(0f, 0.851f, 0f);

        // patch to fix health bars slowly climbing up and up
        static void Postfix(CharacterItem __instance)
        {
			if (__instance.transform.root.GetComponent<Player>())
			{
				if (__instance.moveHealthBarUp != 0f)
				{
					HealthBar componentInChildren = __instance.transform.root.GetComponentInChildren<HealthBar>(true);
					if (componentInChildren)
					{
						componentInChildren.transform.localPosition = defaultHealthbarOffset + Vector3.up * __instance.moveHealthBarUp;
					}
				}
			}
		}
    }
}
