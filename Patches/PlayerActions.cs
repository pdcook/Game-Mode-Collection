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
            __instance.GetAdditionalData().interact = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Interact" });
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
            __instance.GetAdditionalData().trt_shop = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Open Shop (TRT)" });
            __instance.GetAdditionalData().discard_last_card = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Drop Last Card" });
        }
    }
    // postfix PlayerActions to add controller controls
    [HarmonyPatch(typeof(PlayerActions), "CreateWithControllerBindings")]
    class PlayerActionsPatchCreateWithControllerBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(InputControlType.RightStickButton);

            __result.GetAdditionalData().interact.AddDefaultBinding(InputControlType.LeftStickButton);

            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(InputControlType.DPadLeft);

            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(InputControlType.DPadRight);

            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(InputControlType.DPadUp);

            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(InputControlType.DPadDown);

            __result.Fire.RemoveBinding(new DeviceBindingSource(InputControlType.Action3));

            __result.GetAdditionalData().trt_shop.AddDefaultBinding(InputControlType.Action3);

            __result.GetAdditionalData().discard_last_card.AddDefaultBinding(InputControlType.Action4);
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

            __result.GetAdditionalData().interact.AddDefaultBinding(Key.Backquote);
            __result.GetAdditionalData().interact.AddDefaultBinding(Key.F);

            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(Key.Key1);
            __result.GetAdditionalData().trt_radio_imwith.AddDefaultBinding(Key.C);

            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(Key.Key2);
            __result.GetAdditionalData().trt_radio_traitor.AddDefaultBinding(Key.R);
            
            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(Key.Key3);
            __result.GetAdditionalData().trt_radio_suspect.AddDefaultBinding(Key.G);

            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(Key.Key4);
            __result.GetAdditionalData().trt_radio_innocent.AddDefaultBinding(Key.X);

            __result.GetAdditionalData().discard_last_card.AddDefaultBinding(Key.Q);
            __result.GetAdditionalData().discard_last_card.AddDefaultBinding(Key.Z);

            __result.GetAdditionalData().trt_shop.AddDefaultBinding(Key.B);
            __result.GetAdditionalData().trt_shop.AddDefaultBinding(Key.Key6);
        }
    }
}
