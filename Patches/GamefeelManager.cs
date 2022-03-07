using HarmonyLib;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(GamefeelManager), nameof(GamefeelManager.GameFeel))]
    [HarmonyPriority(Priority.First)]
    class GamefeelManagerPatchGameFeel
    {
        private static bool Prefix()
        {
            return !GameModeCollection.IgnoreGameFeel;
        }
    }
    [HarmonyPatch(typeof(GamefeelManager), nameof(GamefeelManager.AddGameFeel))]
    [HarmonyPriority(Priority.First)]
    class GamefeelManagerPatchAddGameFeel
    {
        private static bool Prefix()
        {
            return !GameModeCollection.IgnoreGameFeel;
        }
    }
}
