using TMPro;
using GameModeCollection;
using HarmonyLib;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(UIHandler), nameof(UIHandler.ShowJoinGameText))]
    static class UIHandlerPatchShowJoinGameText
    {
        static void Prefix(UIHandler __instance)
        {
            __instance.jointGameText.font = GameModeCollection.TRT_Assets.LoadAsset<TMP_FontAsset>("Gravity-Light SDF");
        }
    }
}
