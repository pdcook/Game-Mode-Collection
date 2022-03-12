using HarmonyLib;
using UnboundLib;
using GameModeCollection.Objects;
using UnityEngine;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(CardBar), "Start")]
    class CardBarPatchStart
    {
        static void Postfix(GameObject ___source)
        {
            ___source.GetOrAddComponent<SelectableCardBarButton>();
        }
            
    }
}
