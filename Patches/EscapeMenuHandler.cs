using System;
using System.Collections.Generic;
using HarmonyLib;
using InControl;
using System.Reflection.Emit;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(EscapeMenuHandler), "Update")]
    internal class EscapeMenuHandlerPatchUpdate
    {
        public static bool PauseWasPressed(InputDevice device)
        {
            return (device.GetControl(InputControlType.Start).WasPressed
                    || device.GetControl(InputControlType.Pause).WasPressed
                    || device.GetControl(InputControlType.System).WasPressed
                    || device.GetControl(InputControlType.Plus).WasPressed
                    || device.GetControl(InputControlType.Home).WasPressed
                    || device.GetControl(InputControlType.Menu).WasPressed
                    );
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var m_get_CommandWasPressed = typeof(InputDevice).GetMethod("get_CommandWasPressed");
            var m_pauseWasPressed = typeof(EscapeMenuHandlerPatchUpdate).GetMethod(nameof(PauseWasPressed));
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(m_get_CommandWasPressed))
                {
                    yield return new CodeInstruction(OpCodes.Call, m_pauseWasPressed);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
