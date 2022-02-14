using System;
using HarmonyLib;
using GameModeCollection.Extensions;
using System.Reflection;
using InControl;

namespace GameModeCollection.Patches
{
    // postfix PlayerActions constructor to add controls
    [HarmonyPatch(typeof(PlayerActions))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { })]
    class PlayerActionsPatchPlayerActions
    {
        private static void Postfix(PlayerActions __instance)
        {
            __instance.GetAdditionalData().trt_inspect_body = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Inspect Body (TRT)" });
            __instance.GetAdditionalData().trt_interact_with_body = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Interact with Body (TRT)" });
            __instance.GetAdditionalData().trt_radio_imwith = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"I'm with ___.\" (TRT)" });
            __instance.GetAdditionalData().trt_radio_suspect = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is suspicious.\" (TRT)" });
            __instance.GetAdditionalData().trt_radio_traitor = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is a traitor!\" (TRT)" });
            __instance.GetAdditionalData().trt_radio_innocent = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is innocent.\" (TRT)" });

        }
    }
    // postfix PlayerActions to add controller controls
    [HarmonyPatch(typeof(PlayerActions), "CreateWithControllerBindings")]
    class PlayerActionsPatchCreateWithControllerBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(InputControlType.RightStickButton);

            __result.GetAdditionalData().trt_interact_with_body.AddDefaultBinding(InputControlType.LeftStickButton);

            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(InputControlType.DPadLeft);

            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(InputControlType.DPadRight);

            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(InputControlType.DPadUp);

            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(InputControlType.DPadDown);
        }
    }
    // postfix PlayerActions to add keyboard controls
    [HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")]
    class PlayerActionsPatchCreateWithKeyboardBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(Key.Key5);
            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(Key.E);

            __result.GetAdditionalData().trt_interact_with_body.AddDefaultBinding(Key.Tilde);
            __result.GetAdditionalData().trt_interact_with_body.AddDefaultBinding(Key.F);

            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(Key.Key1);
            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(Key.C);

            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(Key.Key2);
            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(Key.R);
            
            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(Key.Key3);
            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(Key.V);

            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(Key.Key4);
            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(Key.X);
        }
    }
}
