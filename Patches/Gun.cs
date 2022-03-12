using GameModeCollection.Extensions;
using HarmonyLib;
namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Gun), nameof(Gun.Attack))]
    class GunPatchAttack
    {
        static bool Prefix(Gun __instance)
        {
            return (!__instance.GetData().disabled && !__instance.GetData().disabledFromCardBar);
        }
    }
}
