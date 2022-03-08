using HarmonyLib;
using GameModeCollection.Extensions;
using LocalZoom;
using UnboundLib;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CharacterCreatorItemEquipper), nameof(CharacterCreatorItemEquipper.EquipFace))]
    class CharacterCreatorItemEquipperPatchEquipFace
    {
        static void Postfix(CharacterCreatorItemEquipper __instance, PlayerFace face)
        {
            __instance?.transform?.root.GetComponent<CharacterData>()?.SetCurrentFace(face);

            GameModeCollection.instance.ExecuteAfterFrames(2, () =>
            {
                if (LocalZoom.LocalZoom.enableShaderSetting && __instance?.transform?.root.GetComponent<CharacterData>() != null)
                {
                    foreach (CharacterItem characterItem in __instance.transform.root.GetComponentsInChildren<CharacterItem>(true))
                    {
                        LocalZoom.LocalZoom.MakeObjectHidden(characterItem);
                    }
                }
            });
        }
    }
}
